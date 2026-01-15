using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 흔하게 쓰이는 예제 Ability들(Heal/Cleanse/StunBolt)을 처리하는 스텁.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityRequestSystem))]
    public class CommonAbilitySystems : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            Entities.WithoutBurst().ForEach((
                Entity entity,
                in AbilitySystemComponent asc,
                DynamicBuffer<GameplayTagElement> tags,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate) =>
            {
                _ = asc;

                // 스턴이면 어떤 능력도 못 쓰는 것으로 처리(요청은 실패로 소비)
                if (HasTag(tags, CommonIds.Tag_Stunned))
                {
                    tryActivate.Clear();
                    return;
                }

                for (int i = tryActivate.Length - 1; i >= 0; i--)
                {
                    var request = tryActivate[i];

                    int abilityId = 0;
                    for (int g = 0; g < granted.Length; g++)
                    {
                        if (granted[g].Handle.Value == request.Handle.Value)
                        {
                            abilityId = granted[g].AbilityId;
                            break;
                        }
                    }

                    if (abilityId == 0)
                        continue;

                    var target = request.Target != Entity.Null ? request.Target : entity;
                    if (request.TargetData.Target != Entity.Null)
                        target = request.TargetData.Target;

                    switch (abilityId)
                    {
                        case CommonIds.Ability_Heal:
                            EnsureBuffer<ApplyEffectRequest>(em, target).Add(new ApplyEffectRequest
                            {
                                Spec = new EffectSpec
                                {
                                    EffectId = CommonIds.Effect_HealInstant,
                                    Level = 1,
                                    Source = entity,
                                    Target = target,
                                    Duration = 0f,
                                    IsPermanent = true,
                                    IsPeriodic = false,
                                    Period = 0f,
                                    GrantedTag = GameplayTag.Invalid,
                                    RevertModifierOnEnd = false,
                                    Modifier = new AttributeModifier
                                    {
                                        Attribute = AttributeId.Health,
                                        Op = AttributeModOp.Add,
                                        Magnitude = 25f,
                                    }
                                }
                            });
                            tryActivate.RemoveAt(i);
                            break;

                        case CommonIds.Ability_Cleanse:
                            // 스텁: Burning 태그 기반으로 효과 제거(DoT 중단) + 태그 제거
                            EnsureBuffer<RemoveEffectsWithTagRequest>(em, target)
                                .Add(new RemoveEffectsWithTagRequest { Tag = new GameplayTag { Value = CommonIds.Tag_Burning } });
                            EnsureBuffer<RemoveGameplayTagRequest>(em, target)
                                .Add(new RemoveGameplayTagRequest { Tag = new GameplayTag { Value = CommonIds.Tag_Burning } });
                            tryActivate.RemoveAt(i);
                            break;

                        case CommonIds.Ability_StunBolt:
                            // 투사체 스폰(비행 시간 후 스턴 적용)
                            var projectile = em.CreateEntity();
                            em.AddComponentData(projectile, new StunBoltProjectile
                            {
                                Source = entity,
                                Target = target,
                                RemainingFlightTime = 0.25f,
                                StunDuration = 2.0f,
                            });
                            tryActivate.RemoveAt(i);
                            break;
                    }
                }
            }).Run();
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

        private static DynamicBuffer<T> EnsureBuffer<T>(EntityManager em, Entity entity) where T : unmanaged, IBufferElementData
        {
            if (!em.HasBuffer<T>(entity))
                em.AddBuffer<T>(entity);
            return em.GetBuffer<T>(entity);
        }
    }
}

