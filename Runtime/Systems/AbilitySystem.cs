using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class AbilitySystem : SystemBase
    {
        [BurstCompile]
        private partial struct AbilitySystemJob : IJobEntity
        {
            public void Execute(ref AbilityBase ability, in EffectBase effect)
            {
                if (ability.IsActive)
                {
                    // 능력을 실행하고 효과를 적용
                    _ = effect;
                }
            }
        }

        protected override void OnUpdate()
        {
            Dependency = new AbilitySystemJob().ScheduleParallel(Dependency);
        }
    }
}