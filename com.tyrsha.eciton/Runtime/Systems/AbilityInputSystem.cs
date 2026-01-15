using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// PressAbilityInputRequest(슬롯 입력)을 TryActivateAbilityRequest(핸들 기반)로 변환하는 코어 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityActivationGateSystem))]
    public class AbilityInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                DynamicBuffer<AbilityInputBinding> bindings,
                DynamicBuffer<PressAbilityInputRequest> inputs,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate) =>
            {
                for (int i = 0; i < inputs.Length; i++)
                {
                    var input = inputs[i];
                    var handle = FindHandle(bindings, input.Slot);
                    if (!handle.IsValid)
                        continue;

                    tryActivate.Add(new TryActivateAbilityRequest
                    {
                        Handle = handle,
                        Target = input.TargetData.Target,
                        TargetData = input.TargetData
                    });
                }

                inputs.Clear();
            }).ScheduleParallel();
        }

        private static AbilityHandle FindHandle(DynamicBuffer<AbilityInputBinding> bindings, AbilityInputSlot slot)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i].Slot == slot)
                    return bindings[i].Handle;
            }
            return AbilityHandle.Invalid;
        }
    }
}

