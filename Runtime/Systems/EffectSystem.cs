using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EffectSystem : ISystem
    {
        [BurstCompile]
        private partial struct EffectJob : IJobEntity
        {
            public float Dt;

            public void Execute(ref EffectBase effect, ref AttributeData attribute)
            {
                // 효과 적용 로직 (예: Attribute 값 변경)
                if (!effect.IsPermanent)
                {
                    effect.Duration -= Dt;
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new EffectJob
            {
                Dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
    }
}
