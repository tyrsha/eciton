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
                in DynamicBuffer<GameplayTagElement> targetTags,
                DynamicBuffer<ActiveEffect> activeEffects,
                DynamicBuffer<ApplyEffectRequest> applyRequests,
                DynamicBuffer<RemoveEffectRequest> removeRequests,
                DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests,
                DynamicBuffer<AddGameplayTagRequest> addTagRequests,
                DynamicBuffer<RemoveGameplayTagRequest> removeTagRequests,
                DynamicBuffer<RemoveEffectsWithTagRequest> removeEffectsByTag) =>
            {
                // Apply
                for (int i = 0; i < applyRequests.Length; i++)
                {
                    var spec = applyRequests[i].Spec;

                    // 면역/차단 태그 체크
                    if (spec.BlockedByTag.IsValid && HasTag(targetTags, spec.BlockedByTag.Value))
                        continue;

                    // 비주기(즉시) 효과: 바로 modifier 적용.
                    if (!spec.IsPeriodic)
                    {
                        ApplyAll(attributeRequests, spec);
                    }

                    // 태그 부여(즉시/지속 상관없이 적용 시점에 1회 추가).
                    if (spec.GrantedTag.IsValid)
                        addTagRequests.Add(new AddGameplayTagRequest { Tag = spec.GrantedTag });

                    // 지속/주기 효과는 ActiveEffect로 관리한다.
                    // (Duration<=0 이면서 비주기면 ActiveEffect를 만들지 않는다.)
                    bool needsActive =
                        spec.IsPeriodic ||
                        (!spec.IsPermanent && spec.Duration > 0f);

                    if (needsActive)
                    {
                        // 스태킹 처리(EffectId + GrantedTag 기준 스텁)
                        if (spec.StackingPolicy != EffectStackingPolicy.None)
                        {
                            for (int e = 0; e < activeEffects.Length; e++)
                            {
                                if (activeEffects[e].EffectId == spec.EffectId &&
                                    activeEffects[e].GrantedTag.Value == spec.GrantedTag.Value)
                                {
                                    var existing = activeEffects[e];
                                    existing.StackingPolicy = spec.StackingPolicy;
                                    existing.MaxStacks = spec.MaxStacks;

                                    int maxStacks = existing.MaxStacks <= 0 ? 1 : existing.MaxStacks;
                                    if (existing.StackCount <= 0) existing.StackCount = 1;

                                    if (spec.StackingPolicy == EffectStackingPolicy.RefreshDuration)
                                    {
                                        if (!existing.IsPermanent)
                                            existing.RemainingTime = spec.Duration;
                                    }
                                    else if (spec.StackingPolicy == EffectStackingPolicy.StackAdditive)
                                    {
                                        if (existing.StackCount < maxStacks)
                                            existing.StackCount++;

                                        // 스텁: 스택이 쌓일 때마다 즉시 modifier를 한 번 더 적용(DoT는 다음 틱부터 자연히 누적됨).
                                        if (!spec.IsPeriodic)
                                            ApplyAll(attributeRequests, spec);

                                        if (!existing.IsPermanent)
                                            existing.RemainingTime = spec.Duration;
                                    }

                                    activeEffects[e] = existing;
                                    // 이미 합쳐졌으므로 신규 생성 스킵
                                    goto Applied;
                                }
                            }
                        }

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
                            Modifiers = spec.Modifiers,
                            IsPeriodic = spec.IsPeriodic,
                            Period = spec.Period,
                            // 스텁: 첫 틱은 Period 이후부터.
                            TimeToNextTick = spec.IsPeriodic ? spec.Period : 0f,
                            GrantedTag = spec.GrantedTag,
                            BlockedByTag = spec.BlockedByTag,
                            RevertModifierOnEnd = spec.RevertModifierOnEnd,
                            StackingPolicy = spec.StackingPolicy,
                            MaxStacks = spec.MaxStacks,
                            StackCount = 1,
                        });
                    }

                Applied:
                    ;
                }

                // Remove
                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var handle = removeRequests[i].Handle;
                    if (!handle.IsValid) continue;

                    for (int e = activeEffects.Length - 1; e >= 0; e--)
                    {
                        if (activeEffects[e].Handle.Value == handle.Value)
                        {
                            // 태그 제거 요청(효과 강제 제거 시)
                            var tag = activeEffects[e].GrantedTag;
                            if (tag.IsValid)
                                removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = tag });
                            activeEffects.RemoveAt(e);
                        }
                    }
                }

                // Remove by tag (cleanse)
                for (int i = 0; i < removeEffectsByTag.Length; i++)
                {
                    var tag = removeEffectsByTag[i].Tag;
                    if (!tag.IsValid) continue;

                    for (int e = activeEffects.Length - 1; e >= 0; e--)
                    {
                        if (activeEffects[e].GrantedTag.Value == tag.Value)
                        {
                            removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = tag });
                            activeEffects.RemoveAt(e);
                        }
                    }
                }

                applyRequests.Clear();
                removeRequests.Clear();
                removeEffectsByTag.Clear();
            }).Schedule();

            _nextHandle = nextHandle;
        }

        private static void ApplyAll(DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests, EffectSpec spec)
        {
            if (spec.Modifiers.Length > 0)
            {
                for (int i = 0; i < spec.Modifiers.Length; i++)
                {
                    var mod = spec.Modifiers[i];
                    if (mod.Magnitude != 0f)
                        attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = mod });
                }
                return;
            }

            if (spec.Modifier.Magnitude != 0f)
                attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = spec.Modifier });
        }

        private static bool HasTag(DynamicBuffer<GameplayTagElement> tags, int tagValue)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i].Tag.Value == tagValue)
                    return true;
            }
            return false;
        }
    }
}

