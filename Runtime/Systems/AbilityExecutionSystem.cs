using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// DB(AbilityDefinition.ExecutionType)에 따라 Ability 실행을 일반화하는 시스템.
    /// - ApplyEffectToTarget: Primary/Secondary effect를 타겟에 적용
    /// - SpawnProjectileAndApplyEffectsOnHit: 범용 투사체 스폰
    /// - CleanseByTag: 정의된 태그 기반으로 효과 제거
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AbilityActivationGateSystem))]
    [UpdateBefore(typeof(FireballAbilitySystem))]
    [UpdateBefore(typeof(CommonAbilitySystems))]
    public class AbilityExecutionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = EntityManager;

            Entities.WithoutBurst().WithStructuralChanges().ForEach((
                Entity entity,
                in AbilitySystemComponent asc,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate) =>
            {
                _ = asc;

                for (int i = tryActivate.Length - 1; i >= 0; i--)
                {
                    var req = tryActivate[i];

                    int abilityId = 0;
                    for (int g = 0; g < granted.Length; g++)
                    {
                        if (granted[g].Handle.Value == req.Handle.Value)
                        {
                            abilityId = granted[g].AbilityId;
                            break;
                        }
                    }

                    if (abilityId == 0)
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    if (!AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var def))
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    var target = req.TargetData.Target != Entity.Null ? req.TargetData.Target : req.Target;
                    if (target == Entity.Null)
                        target = entity;

                    switch (def.ExecutionType)
                    {
                        case AbilityExecutionType.ApplyEffectToTarget:
                            ApplyEffectsById(em, entity, target, def.PrimaryEffectId, def.SecondaryEffectId);
                            break;

                        case AbilityExecutionType.SpawnProjectileAndApplyEffectsOnHit:
                            {
                                var projectile = em.CreateEntity();
                                em.AddComponentData(projectile, new AbilityProjectile
                                {
                                    Source = entity,
                                    Target = target,
                                    RemainingFlightTime = def.ProjectileFlightTime,
                                    PrimaryEffectId = def.PrimaryEffectId,
                                    SecondaryEffectId = def.SecondaryEffectId
                                });
                            }
                            break;

                        case AbilityExecutionType.CleanseByTag:
                            if (def.CleanseTag.IsValid)
                            {
                                EnsureBuffer<RemoveEffectsWithTagRequest>(em, target)
                                    .Add(new RemoveEffectsWithTagRequest { Tag = def.CleanseTag });
                                EnsureBuffer<RemoveGameplayTagRequest>(em, target)
                                    .Add(new RemoveGameplayTagRequest { Tag = def.CleanseTag });
                            }
                            break;
                    }

                    // 실행 완료: 요청 소비
                    tryActivate.RemoveAt(i);
                }
            }).Run();
        }

        private static void ApplyEffectsById(EntityManager em, Entity source, Entity target, int primary, int secondary)
        {
            if (!em.HasBuffer<ApplyEffectByIdRequest>(target))
                em.AddBuffer<ApplyEffectByIdRequest>(target);
            var effects = em.GetBuffer<ApplyEffectByIdRequest>(target);

            if (primary != 0)
                effects.Add(new ApplyEffectByIdRequest { EffectId = primary, Level = 1, Source = source, Target = target });
            if (secondary != 0)
                effects.Add(new ApplyEffectByIdRequest { EffectId = secondary, Level = 1, Source = source, Target = target });
        }

        private static DynamicBuffer<T> EnsureBuffer<T>(EntityManager em, Entity entity) where T : unmanaged, IBufferElementData
        {
            if (!em.HasBuffer<T>(entity))
                em.AddBuffer<T>(entity);
            return em.GetBuffer<T>(entity);
        }
    }
}

