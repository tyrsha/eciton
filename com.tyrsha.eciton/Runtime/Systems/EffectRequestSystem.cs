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

                    // 비주기(즉시) 효과: 바로 modifier 적용.
                    if (!spec.IsPeriodic)
                    {
                        attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = spec.Modifier });
                    }

                    // 지속/주기 효과는 ActiveEffect로 관리한다.
                    // (Duration<=0 이면서 비주기면 ActiveEffect를 만들지 않는다.)
                    bool needsActive =
                        spec.IsPeriodic ||
                        (!spec.IsPermanent && spec.Duration > 0f);

                    if (needsActive)
                    {
                        var handle = new EffectHandle { Value = nextHandle++ };
                        activeEffects.Add(new ActiveEffect
                        {
                            Handle = handle,
                            EffectId = spec.EffectId,
                            Level = spec.Level,
                            Source = spec.Source,
                            RemainingTime = spec.IsPermanent ? 0f : spec.Duration,
                            IsPermanent = spec.IsPermanent,
                            Modifier = spec.Modifier,
                            IsPeriodic = spec.IsPeriodic,
                            Period = spec.Period,
                            // 스텁: 첫 틱은 Period 이후부터.
                            TimeToNextTick = spec.IsPeriodic ? spec.Period : 0f,
                        });
                    }
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

