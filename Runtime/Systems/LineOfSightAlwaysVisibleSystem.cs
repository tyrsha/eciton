using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// LOS 훅 기본 구현(스텁): 반경 내 타겟을 모두 "보임"으로 처리.
    /// 프로젝트에서는 물리/레이캐스트 기반 LOS 시스템으로 대체하는 것을 권장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PerceptionSystem))]
    public class LineOfSightAlwaysVisibleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            using var targetQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<Targetable>(),
                ComponentType.ReadOnly<Faction>(),
                ComponentType.ReadOnly<LocalTransform>());

            var targets = targetQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            var targetFactions = targetQuery.ToComponentDataArray<Faction>(Unity.Collections.Allocator.Temp);
            var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.Temp);

            Entities.WithoutBurst().ForEach((Entity self, in Faction faction, in PerceptionSensor sensor, in LocalTransform xform) =>
            {
                if (sensor.RequireLineOfSight == 0)
                    return;

                if (!em.HasBuffer<VisibleTarget>(self))
                    em.AddBuffer<VisibleTarget>(self);
                var visible = em.GetBuffer<VisibleTarget>(self);
                visible.Clear();

                float radiusSq = sensor.Radius * sensor.Radius;
                float3 pos = xform.Position;

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targetFactions[i].Value == faction.Value)
                        continue;
                    float distSq = math.lengthsq(targetTransforms[i].Position - pos);
                    if (distSq > radiusSq)
                        continue;
                    visible.Add(new VisibleTarget { Target = targets[i] });
                }
            }).Run();

            targets.Dispose();
            targetFactions.Dispose();
            targetTransforms.Dispose();
        }
    }
}

