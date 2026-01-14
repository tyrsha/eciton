using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Effect 적용/제거 요청을 처리하는 최소 스텁 시스템.
    /// 실제 스택/태그/면역/예외 규칙 등은 이후 확장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EffectRequestSystem : ISystem
    {
        private int _nextHandle;

        [BurstCompile]
        private partial struct EffectRequestJob : IJobEntity
        {
            public AbilityEffectDatabase Db;
            public NativeReference<int> NextHandle;

            public void Execute(
                Entity entity,
                in DynamicBuffer<GameplayTagElement> targetTags,
                DynamicBuffer<ActiveEffect> activeEffects,
                DynamicBuffer<ApplyEffectRequest> applyRequests,
                DynamicBuffer<RemoveEffectRequest> removeRequests,
                DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests,
                DynamicBuffer<AddGameplayTagRequest> addTagRequests,
                DynamicBuffer<RemoveGameplayTagRequest> removeTagRequests,
                DynamicBuffer<RemoveEffectsWithTagRequest> removeEffectsByTag,
                DynamicBuffer<PendingGameplayEvent> events)
            {
                int nextHandle = NextHandle.Value;
                
                if (!Db.Blob.IsCreated)
                {
                    applyRequests.Clear();
                    removeRequests.Clear();
                    removeEffectsByTag.Clear();
                    NextHandle.Value = nextHandle;
                    return;
                }
                
                ref var effects = ref Db.Blob.Value.Effects;

                // Apply
                for (int i = 0; i < applyRequests.Length; i++)
                {
                    var spec = applyRequests[i].Spec;
                    bool found = false;
                    
                    for (int j = 0; j < effects.Length; j++)
                    {
                        if (effects[j].EffectId == spec.EffectId)
                        {
                            ref var def = ref effects[j];
                            found = true;

                            // 면역/차단 태그 체크
                            if (def.BlockedByTag.IsValid && HasTag(targetTags, def.BlockedByTag.Value))
                                break;

                            // 비주기(즉시) 효과: 바로 modifier 적용.
                            if (!def.IsPeriodic)
                            {
                                ApplyAll(attributeRequests, ref def, 1);
                            }

                            // 태그 부여(즉시/지속 상관없이 적용 시점에 1회 추가).
                            if (def.GrantedTag.IsValid)
                                addTagRequests.Add(new AddGameplayTagRequest { Tag = def.GrantedTag });

                            // 이벤트: effect applied (스텁)
                            events.Add(new PendingGameplayEvent
                            {
                                Event = new GameplayEvent
                                {
                                    Type = GameplayEventType.EffectApplied,
                                    Source = spec.Source,
                                    Target = entity,
                                    Id = spec.EffectId,
                                    Magnitude = 0f
                                }
                            });

                            // 지속/주기 효과는 ActiveEffect로 관리한다.
                            // (Duration<=0 이면서 비주기면 ActiveEffect를 만들지 않는다.)
                            bool needsActive =
                                def.IsPeriodic ||
                                (!def.IsPermanent && def.Duration > 0f);

                            if (needsActive)
                            {
                                // 스태킹 처리(EffectId + GrantedTag 기준 스텁)
                                bool stacked = false;
                                if (def.StackingPolicy != EffectStackingPolicy.None)
                                {
                                    for (int e = 0; e < activeEffects.Length; e++)
                                    {
                                        // EffectId + GrantedTag 기준 merge
                                        if (activeEffects[e].EffectId == def.EffectId)
                                        {
                                            var existing = activeEffects[e];
                                            existing.StackingPolicy = def.StackingPolicy;
                                            existing.MaxStacks = def.MaxStacks;

                                            int maxStacks = existing.MaxStacks <= 0 ? 1 : existing.MaxStacks;
                                            if (existing.StackCount <= 0) existing.StackCount = 1;

                                            if (def.StackingPolicy == EffectStackingPolicy.RefreshDuration)
                                            {
                                                if (!def.IsPermanent)
                                                    existing.RemainingTime = def.Duration;
                                            }
                                            else if (def.StackingPolicy == EffectStackingPolicy.StackAdditive)
                                            {
                                                if (existing.StackCount < maxStacks)
                                                    existing.StackCount++;

                                                // 스텁: 스택이 쌓일 때마다 즉시 modifier를 한 번 더 적용(DoT는 다음 틱부터 자연히 누적됨).
                                                if (!def.IsPeriodic)
                                                    ApplyAll(attributeRequests, ref def, 1);

                                                if (!def.IsPermanent)
                                                    existing.RemainingTime = def.Duration;
                                            }

                                            activeEffects[e] = existing;
                                            // 이미 합쳐졌으므로 신규 생성 스킵
                                            stacked = true;
                                            break;
                                        }
                                    }
                                }

                                if (!stacked)
                                {
                                    var handle = new EffectHandle { Value = nextHandle++ };
                                    activeEffects.Add(new ActiveEffect
                                    {
                                        Handle = handle,
                                        EffectId = def.EffectId,
                                        Level = spec.Level,
                                        Source = spec.Source,
                                        RemainingTime = def.IsPermanent ? 0f : def.Duration,
                                        TimeToNextTick = def.IsPeriodic ? def.Period : 0f,
                                        StackingPolicy = def.StackingPolicy,
                                        MaxStacks = def.MaxStacks,
                                        StackCount = 1,
                                    });
                                }
                            }

                            break;
                        }
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
                        {
                            // 태그 제거 요청(효과 강제 제거 시)
                            int removedEffectId = activeEffects[e].EffectId;
                            
                            for (int j = 0; j < effects.Length; j++)
                            {
                                if (effects[j].EffectId == removedEffectId)
                                {
                                    ref var removedDef = ref effects[j];
                                    if (removedDef.GrantedTag.IsValid)
                                        removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = removedDef.GrantedTag });
                                    break;
                                }
                            }
                            
                            activeEffects.RemoveAt(e);

                            events.Add(new PendingGameplayEvent
                            {
                                Event = new GameplayEvent
                                {
                                    Type = GameplayEventType.EffectRemoved,
                                    Source = Entity.Null,
                                    Target = entity,
                                    Id = removedEffectId,
                                    Magnitude = 0f
                                }
                            });
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
                        int effectId = activeEffects[e].EffectId;
                        
                        for (int j = 0; j < effects.Length; j++)
                        {
                            if (effects[j].EffectId == effectId)
                            {
                                ref var cleanseDef = ref effects[j];
                                if (cleanseDef.GrantedTag.Value == tag.Value)
                                {
                                    removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = tag });
                                    activeEffects.RemoveAt(e);
                                }
                                break;
                            }
                        }
                    }
                }

                applyRequests.Clear();
                removeRequests.Clear();
                removeEffectsByTag.Clear();

                NextHandle.Value = nextHandle;
            }

            private static void ApplyAll(DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests, ref EffectDefinition def, int stackCount)
            {
                int count = def.Modifiers.Length;
                for (int i = 0; i < count; i++)
                {
                    var mod = def.Modifiers[i];
                    if (mod.Magnitude == 0f)
                        continue;
                    if (stackCount != 1)
                        mod.Magnitude *= stackCount;
                    attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = mod });
                }
            }

            private static bool HasTag(in DynamicBuffer<GameplayTagElement> tags, int tagValue)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].Tag.Value == tagValue)
                        return true;
                }
                return false;
            }
        }

        public void OnCreate(ref SystemState state)
        {
            _nextHandle = 1;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var nextHandleRef = new NativeReference<int>(AllocatorManager.TempJob);
            nextHandleRef.Value = _nextHandle;

            state.Dependency = new EffectRequestJob
            {
                Db = db,
                NextHandle = nextHandleRef
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
            _nextHandle = nextHandleRef.Value;
            nextHandleRef.Dispose();
        }
    }
}
