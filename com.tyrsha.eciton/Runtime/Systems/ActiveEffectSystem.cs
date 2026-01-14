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
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            float dt = Time.DeltaTime;

            Entities.ForEach((
                DynamicBuffer<ActiveEffect> activeEffects,
                DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests,
                DynamicBuffer<RemoveGameplayTagRequest> removeTagRequests) =>
            {
                for (int i = activeEffects.Length - 1; i >= 0; i--)
                {
                    var effect = activeEffects[i];
                    if (!AbilityEffectDatabaseLookup.TryGetEffect(db, effect.EffectId, out var def))
                    {
                        activeEffects.RemoveAt(i);
                        continue;
                    }

                    if (def.IsPeriodic)
                    {
                        float period = def.Period;
                        if (period > 0f)
                        {
                            effect.TimeToNextTick -= dt;
                            while (effect.TimeToNextTick <= 0f)
                            {
                                ApplyAll(attributeRequests, def, effect.StackCount <= 0 ? 1 : effect.StackCount);
                                effect.TimeToNextTick += period;
                            }
                        }
                    }

                    if (!def.IsPermanent)
                    {
                        effect.RemainingTime -= dt;
                        if (effect.RemainingTime <= 0f)
                        {
                            // 만료 시 태그 제거
                            if (def.GrantedTag.IsValid)
                                removeTagRequests.Add(new RemoveGameplayTagRequest { Tag = def.GrantedTag });

                            // 만료 시 지속형(비주기) modifier 되돌리기
                            if (!def.IsPeriodic && def.RevertModifierOnEnd)
                            {
                                RevertAll(attributeRequests, def, effect.StackCount <= 0 ? 1 : effect.StackCount);
                            }
                            activeEffects.RemoveAt(i);
                            continue;
                        }
                    }

                    activeEffects[i] = effect;
                }
            }).Schedule();
        }

        private static AttributeModifier Invert(AttributeModifier mod)
        {
            switch (mod.Op)
            {
                case AttributeModOp.Add:
                    mod.Magnitude = -mod.Magnitude;
                    return mod;
                case AttributeModOp.Multiply:
                    if (mod.Magnitude != 0f)
                        mod.Magnitude = 1f / mod.Magnitude;
                    return mod;
                case AttributeModOp.Override:
                default:
                    // 스텁: Override는 되돌리기 불가로 취급
                    mod.Magnitude = 0f;
                    return mod;
            }
        }

        private static void ApplyAll(DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests, EffectDefinition def, int stackCount)
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

        private static void RevertAll(DynamicBuffer<ApplyAttributeModifierRequest> attributeRequests, EffectDefinition def, int stackCount)
        {
            int count = def.Modifiers.Length;
            for (int i = 0; i < count; i++)
            {
                var inv = Invert(def.Modifiers[i]);
                if (inv.Magnitude == 0f)
                    continue;
                if (stackCount != 1)
                    inv.Magnitude *= stackCount;
                attributeRequests.Add(new ApplyAttributeModifierRequest { Modifier = inv });
            }
        }
    }
}

