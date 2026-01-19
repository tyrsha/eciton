using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Ability 활성화/취소 요청을 소비하는 최소 스텁 시스템.
    /// 실제 Ability 실행 로직은 이후 AbilitySystem 확장으로 구현.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AbilityRequestSystem : ISystem
    {
        [BurstCompile]
        private partial struct AbilityRequestConsumeJob : IJobEntity
        {
            public void Execute(
                in AbilitySystemComponent asc,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate,
                DynamicBuffer<CancelAbilityRequest> cancel)
            {
                // NOTE: 스텁에서는 요청을 단순 소비만 한다.
                // 이후 구현에서는 granted에서 spec을 찾고, 비용/쿨다운/태그 등을 검사한 뒤 실행한다.
                _ = asc;
                _ = granted;

                tryActivate.Clear();
                cancel.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AbilityRequestConsumeJob().Schedule(state.Dependency);
        }
    }
}

