using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// LOS 훅 기본 구현(스텁): 반경 내 타겟을 모두 "보임"으로 처리.
    /// 프로젝트에서는 물리/레이캐스트 기반 LOS 시스템으로 대체하는 것을 권장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PerceptionSystem))]
    public partial struct LineOfSightAlwaysVisibleSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            var targetQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<Targetable>(),
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<LocalTransform>());

            var targets = targetQuery.ToEntityArray(AllocatorManager.Temp);
            var targetFactions = targetQuery.ToComponentDataArray<Faction>(AllocatorManager.Temp);
            var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(AllocatorManager.Temp);

            // 그 다음 버퍼 채우기 (NativeArray를 람다 밖에서 처리)
            var selfQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<PerceptionSensor>(),
                ComponentType.ReadOnly<LocalTransform>());
            var selfEntities = selfQuery.ToEntityArray(AllocatorManager.Temp);
            var selfFactions = selfQuery.ToComponentDataArray<Faction>(AllocatorManager.Temp);
            var selfSensors = selfQuery.ToComponentDataArray<PerceptionSensor>(AllocatorManager.Temp);
            var selfTransforms = selfQuery.ToComponentDataArray<LocalTransform>(AllocatorManager.Temp);

            for (int s = 0; s < selfEntities.Length; s++)
            {
                var self = selfEntities[s];
                var sensor = selfSensors[s];
                
                if (sensor.RequireLineOfSight == 0)
                    continue;

                if (!em.HasBuffer<VisibleTarget>(self))
                    em.AddBuffer<VisibleTarget>(self);

                var visible = em.GetBuffer<VisibleTarget>(self);
                visible.Clear();

                float radiusSq = sensor.Radius * sensor.Radius;
                float3 pos = selfTransforms[s].Position;
                int factionValue = selfFactions[s].Value;

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targetFactions[i].Value == factionValue)
                        continue;
                    float distSq = math.lengthsq(targetTransforms[i].Position - pos);
                    if (distSq > radiusSq)
                        continue;
                    visible.Add(new VisibleTarget { Target = targets[i] });
                }
            }

            selfEntities.Dispose();
            selfFactions.Dispose();
            selfSensors.Dispose();
            selfTransforms.Dispose();
            targets.Dispose();
            targetFactions.Dispose();
            targetTransforms.Dispose();
        }
    }
}

