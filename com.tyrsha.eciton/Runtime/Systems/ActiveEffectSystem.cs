using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// ActiveEffect의 시간 경과/만료 및 주기(DoT/HoT) 틱을 처리하는 최소 스텁 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    public class ActiveEffectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;

            Entities.ForEach((
                DynamicBuffer<ActiveEffect> activeEffects,
                DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests,
                DynamicBuffer<RemoveGameplayTagRequest> removeTagRequests) =>
            {
                for (int i = activeEffects.Length - 1; i >= 0; i--)
                {
                    var effect = activeEffects[i];

                    if (effect.IsPeriodic)
                    {
                        float period = effect.Period;
                        if (period > 0f)
                        {
                            effect.TimeToNextTick -= dt;
                            while (effect.TimeToNextTick <= 0f)
                            {
                                attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = effect.Modifier });
                                effect.TimeToNextTick += period;
                            }
                        }
                    }

                    if (!effect.IsPermanent)
                    {
                        effect.RemainingTime -= dt;
                        if (effect.RemainingTime <= 0f)
                        {
                            // 만료 시 태그 제거
                            if (effect.GrantedTag.IsValid)
                                removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = effect.GrantedTag });
                            activeEffects.RemoveAt(i);
                            continue;
                        }
                    }

                    activeEffects[i] = effect;
                }
            }).Schedule();
        }
    }
}

