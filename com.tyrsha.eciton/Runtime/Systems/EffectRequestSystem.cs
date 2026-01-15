using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Effect 적용/제거 요청을 처리하는 최소 스텁 시스템.
    /// 실제 스택/태그/면역/예외 규칙 등은 이후 확장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class EffectRequestSystem : SystemBase
    {
        private int _nextHandle;

        protected override void OnCreate()
        {
            base.OnCreate();
            _nextHandle = 1;
        }

        protected override void OnUpdate()
        {
            int nextHandle = _nextHandle;

            Entities.ForEach((
                DynamicBuffer<ActiveEffect> activeEffects,
                DynamicBuffer<ApplyEffectRequest> applyRequests,
                DynamicBuffer<RemoveEffectRequest> removeRequests,
                DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests) =>
            {
                // Apply
                for (int i = 0; i < applyRequests.Length; i++)
                {
                    var spec = applyRequests[i].Spec;

                    var handle = new EffectHandle { Value = nextHandle++ };
                    activeEffects.Add(new ActiveEffect
                    {
                        Handle = handle,
                        EffectId = spec.EffectId,
                        Level = spec.Level,
                        Source = spec.Source,
                        RemainingTime = spec.IsPermanent ? 0f : spec.Duration,
                        IsPermanent = spec.IsPermanent,
                    });

                    // 스텁: 즉시 Attribute 변경 요청 1개를 발생시킨다(지속형/주기형 등은 이후 확장).
                    attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = spec.Modifier });
                }

                // Remove
                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var handle = removeRequests[i].Handle;
                    if (!handle.IsValid) continue;

                    for (int e = activeEffects.Length - 1; e >= 0; e--)
                    {
                        if (activeEffects[e].Handle.Value == handle.Value)
                            activeEffects.RemoveAt(e);
                    }
                }

                applyRequests.Clear();
                removeRequests.Clear();
            }).Schedule();

            _nextHandle = nextHandle;
        }
    }
}

