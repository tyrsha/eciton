using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 몬스터 인지(Perception) 스텁:
    /// - 가장 가까운 Targetable(다른 Faction)을 반경 내에서 선택
    /// - BehaviorTreeBlackboard.Target / TargetInRange를 갱신
    /// 실제 게임에서는 시야각/LOS/위협도/가중치 등을 추가 확장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(BehaviorTreeTickSystem))]
    public partial struct PerceptionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            double now = SystemAPI.Time.ElapsedTime;

            // 타겟 후보 캐시(스텁: 매 프레임 배열로 가져옴)
            var targetQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<Targetable>(),
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<LocalTransform>());

            var targets = targetQuery.ToEntityArray(Allocator.Temp);
            var targetFactions = targetQuery.ToComponentDataArray<Faction>(Allocator.Temp);
            var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            var selfQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<BehaviorTreeBlackboard>(),
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<PerceptionSensor>(),
                ComponentType.ReadOnly<AttackRange>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadWrite<ThreatEntry>());
            using var selfEntities = selfQuery.ToEntityArray(Allocator.Temp);

            for (int s = 0; s < selfEntities.Length; s++)
            {
                var self = selfEntities[s];
                var bb = em.GetComponentData<BehaviorTreeBlackboard>(self);
                var faction = em.GetComponentData<Faction>(self);
                var sensor = em.GetComponentData<PerceptionSensor>(self);
                var range = em.GetComponentData<AttackRange>(self);
                var xform = em.GetComponentData<LocalTransform>(self);
                var threat = em.GetBuffer<ThreatEntry>(self);

                float bestScore = float.MinValue;
                float bestDistSq = float.MaxValue;
                Entity best = bb.Target; // 히스테리시스용

                float radiusSq = sensor.Radius * sensor.Radius;
                float3 pos = xform.Position;
                float distWeight = sensor.DistanceWeight;
                float hysteresis = sensor.SwitchHysteresis;

                // LOS 후보 제한(훅)
                bool requireLos = sensor.RequireLineOfSight != 0;
                DynamicBuffer<VisibleTarget> visible = default;
                bool hasVisibleBuffer = requireLos && em.HasBuffer<VisibleTarget>(self);
                if (hasVisibleBuffer)
                    visible = em.GetBuffer<VisibleTarget>(self);

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targetFactions[i].Value == faction.Value)
                        continue;

                    float3 tpos = targetTransforms[i].Position;
                    float distSq = math.lengthsq(tpos - pos);
                    if (distSq > radiusSq)
                        continue;

                    var candidate = targets[i];

                    if (hasVisibleBuffer && !IsVisible(visible, candidate))
                        continue;

                    float threatValue = GetThreat(threat, candidate, now);
                    float priority = em.HasComponent<TargetPriority>(candidate) ? em.GetComponentData<TargetPriority>(candidate).Weight : 0f;
                    float score = threatValue + priority - (distSq * distWeight);

                    if (score > bestScore)
                    {
                        bestDistSq = distSq;
                        best = candidate;
                        bestScore = score;
                    }
                }

                // 히스테리시스: 기존 타겟이 충분히 괜찮으면 유지
                if (bb.Target != Entity.Null && em.Exists(bb.Target))
                {
                    float currentDistSq = float.MaxValue;
                    if (em.HasComponent<LocalTransform>(bb.Target))
                    {
                        float3 tpos = em.GetComponentData<LocalTransform>(bb.Target).Position;
                        currentDistSq = math.lengthsq(tpos - pos);
                    }

                    float currentThreat = GetThreat(threat, bb.Target, now);
                    float currentPriority = em.HasComponent<TargetPriority>(bb.Target) ? em.GetComponentData<TargetPriority>(bb.Target).Weight : 0f;
                    float currentScore = currentThreat + currentPriority - (currentDistSq * distWeight);

                    // 메모리: 반경 밖이어도 최근에 봤으면 유지
                    bool withinRadiusOrRemembered = currentDistSq <= radiusSq || IsRemembered(threat, bb.Target, now, sensor.MemorySeconds);

                    if (withinRadiusOrRemembered)
                    {
                        // LOS 제한이 있고 visible buffer가 있으면, 안 보이면 스위치 고려(메모리로 잠깐 유지 가능)
                        bool currentVisibleOk = !hasVisibleBuffer || IsVisible(visible, bb.Target) || IsRemembered(threat, bb.Target, now, sensor.MemorySeconds);
                        if (currentVisibleOk)
                        {
                            if (best == Entity.Null || currentScore + hysteresis >= bestScore)
                            {
                                best = bb.Target;
                                bestDistSq = currentDistSq;
                            }
                        }
                    }
                }

                bb.Target = best;
                float attackRange = range.Value <= 0f ? 0f : range.Value;
                bb.TargetInRange = (best != Entity.Null && attackRange > 0f && bestDistSq <= attackRange * attackRange) ? (byte)1 : (byte)0;
                em.SetComponentData(self, bb);
            }

            targets.Dispose();
            targetFactions.Dispose();
            targetTransforms.Dispose();
        }

        private static bool IsVisible(DynamicBuffer<VisibleTarget> visible, Entity target)
        {
            for (int i = 0; i < visible.Length; i++)
                if (visible[i].Target == target)
                    return true;
            return false;
        }

        private static float GetThreat(DynamicBuffer<ThreatEntry> threat, Entity target, double now)
        {
            for (int i = 0; i < threat.Length; i++)
            {
                var e = threat[i];
                if (e.Target == target)
                {
                    // seen time 갱신은 Perception이 관장(현재는 후보면 갱신)
                    e.LastSeenTime = now;
                    threat[i] = e;
                    return e.Threat;
                }
            }
            return 0f;
        }

        private static bool IsRemembered(DynamicBuffer<ThreatEntry> threat, Entity target, double now, float memorySeconds)
        {
            if (memorySeconds <= 0f)
                return false;
            for (int i = 0; i < threat.Length; i++)
            {
                var e = threat[i];
                if (e.Target == target)
                {
                    if (e.LastSeenTime <= 0) return false;
                    return (now - e.LastSeenTime) <= memorySeconds;
                }
            }
            return false;
        }
    }
}

