using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// AbilityInputBindingByAbilityId(슬롯->AbilityId)를 기반으로
    /// AbilityInputBinding(슬롯->Handle)을 자동으로 맞춰주는 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AbilityGrantSystem))]
    [UpdateBefore(typeof(AbilityInputSystem))]
    public partial struct AbilityInputAutoBindSystem : ISystem
    {
        [BurstCompile]
        private partial struct AbilityInputAutoBindJob : IJobEntity
        {
            public void Execute(
                DynamicBuffer<AbilityInputBindingByAbilityId> desired,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<AbilityInputBinding> bindings)
            {
                for (int i = 0; i < desired.Length; i++)
                {
                    var d = desired[i];
                    if (d.AbilityId == 0)
                        continue;

                    // 이미 바인딩돼 있으면 스킵
                    bool has = false;
                    for (int b = 0; b < bindings.Length; b++)
                    {
                        if (bindings[b].Slot == d.Slot)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (has)
                        continue;

                    // 부여된 Ability 중 AbilityId가 맞는 첫 Handle로 바인딩
                    for (int g = 0; g < granted.Length; g++)
                    {
                        if (granted[g].AbilityId == d.AbilityId)
                        {
                            bindings.Add(new AbilityInputBinding { Slot = d.Slot, Handle = granted[g].Handle });
                            break;
                        }
                    }
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AbilityInputAutoBindJob().ScheduleParallel(state.Dependency);
        }
    }
}

