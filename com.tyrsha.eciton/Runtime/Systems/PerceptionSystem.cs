using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
    public class PerceptionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            // 타겟 후보 캐시(스텁: 매 프레임 배열로 가져옴)
            using var targetQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<Targetable>(),
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<LocalTransform>());

            var targets = targetQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            var targetFactions = targetQuery.ToComponentDataArray<Faction>(Unity.Collections.Allocator.Temp);
            var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.Temp);

            Entities.WithoutBurst().ForEach((ref BehaviorTreeBlackboard bb, in Faction faction, in PerceptionSensor sensor, in AttackRange range, in LocalTransform xform) =>
            {
                float bestDistSq = float.MaxValue;
                Entity best = Entity.Null;
                float radiusSq = sensor.Radius * sensor.Radius;
                float3 pos = xform.Position;

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targetFactions[i].Value == faction.Value)
                        continue;

                    float3 tpos = targetTransforms[i].Position;
                    float distSq = math.lengthsq(tpos - pos);
                    if (distSq > radiusSq)
                        continue;

                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        best = targets[i];
                    }
                }

                bb.Target = best;
                float attackRange = range.Value <= 0f ? 0f : range.Value;
                bb.TargetInRange = (best != Entity.Null && attackRange > 0f && bestDistSq <= attackRange * attackRange) ? (byte)1 : (byte)0;
            }).Run();

            targets.Dispose();
            targetFactions.Dispose();
            targetTransforms.Dispose();
        }
    }
}

