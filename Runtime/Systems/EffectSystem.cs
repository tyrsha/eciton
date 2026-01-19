using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class EffectSystem : SystemBase
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

        protected override void OnUpdate()
        {
            Dependency = new EffectJob
            {
                Dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(Dependency);
        }
    }
}
