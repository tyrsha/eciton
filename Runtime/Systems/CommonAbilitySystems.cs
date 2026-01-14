using Unity.Entities;
using Unity.Collections;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 흔하게 쓰이는 예제 Ability들(Heal/Cleanse/StunBolt)을 처리하는 스텁.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityRequestSystem))]
    [UpdateAfter(typeof(AbilityExecutionSystem))]
    public partial struct CommonAbilitySystems : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = state.EntityManager;

            var query = state.GetEntityQuery(
                ComponentType.ReadOnly<AbilitySystemComponent>(),
                ComponentType.ReadWrite<GameplayTagElement>(),
                ComponentType.ReadWrite<GrantedAbility>(),
                ComponentType.ReadWrite<TryActivateAbilityRequest>());
            using var entities = query.ToEntityArray(AllocatorManager.Temp);

            for (int q = 0; q < entities.Length; q++)
            {
                var entity = entities[q];
                _ = em.GetComponentData<AbilitySystemComponent>(entity);
                var tags = em.GetBuffer<GameplayTagElement>(entity);
                var granted = em.GetBuffer<GrantedAbility>(entity);
                var tryActivate = em.GetBuffer<TryActivateAbilityRequest>(entity);

                // 스턴이면 어떤 능력도 못 쓰는 것으로 처리(요청은 실패로 소비)
                if (HasTag(tags, CommonIds.Tag_Stunned))
                {
                    tryActivate.Clear();
                    continue;
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
                            if (AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var healDef) && healDef.PrimaryEffectId != 0)
                            {
                                EnsureBuffer<ApplyEffectByIdRequest>(em, target).Add(new ApplyEffectByIdRequest
                                {
                                    EffectId = healDef.PrimaryEffectId,
                                    Level = 1,
                                    Source = entity,
                                    Target = target
                                });
                            }
                            tryActivate.RemoveAt(i);
                            break;

                        case CommonIds.Ability_Cleanse:
                            // 스텁: 정의된 태그 기반으로 효과 제거 + 태그 제거
                            if (AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var cleanseDef) && cleanseDef.CleanseTag.IsValid)
                            {
                                EnsureBuffer<RemoveEffectsWithTagRequest>(em, target)
                                    .Add(new RemoveEffectsWithTagRequest { Tag = cleanseDef.CleanseTag });
                                EnsureBuffer<RemoveGameplayTagRequest>(em, target)
                                    .Add(new RemoveGameplayTagRequest { Tag = cleanseDef.CleanseTag });
                            }
                            tryActivate.RemoveAt(i);
                            break;

                        case CommonIds.Ability_StunBolt:
                            if (AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var stunDef))
                            {
                                // 투사체 스폰(비행 시간 후 스턴 적용)
                                var projectile = em.CreateEntity();
                                em.AddComponentData(projectile, new StunBoltProjectile
                                {
                                    Source = entity,
                                    Target = target,
                                    RemainingFlightTime = stunDef.ProjectileFlightTime,
                                    EffectId = stunDef.PrimaryEffectId,
                                });
                            }
                            tryActivate.RemoveAt(i);
                            break;
                    }
                }
            }
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

