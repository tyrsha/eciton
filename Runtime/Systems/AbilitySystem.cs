using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AbilitySystem : ISystem
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

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AbilitySystemJob().ScheduleParallel(state.Dependency);
        }
    }
}