using Unity.Entities;
using Unity.Collections;

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
    public partial struct AbilityExecutionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = state.EntityManager;

            var query = state.GetEntityQuery(
                ComponentType.ReadOnly<AbilitySystemComponent>(),
                ComponentType.ReadWrite<GrantedAbility>(),
                ComponentType.ReadWrite<TryActivateAbilityRequest>());
            using var entities = query.ToEntityArray(AllocatorManager.Temp);

            for (int q = 0; q < entities.Length; q++)
            {
                var entity = entities[q];
                _ = em.GetComponentData<AbilitySystemComponent>(entity);
                var granted = em.GetBuffer<GrantedAbility>(entity);
                var tryActivate = em.GetBuffer<TryActivateAbilityRequest>(entity);

                // 처리할 요청의 핸들을 저장 (구조적 변경 후 제거하기 위해)
                using var handlesToRemove = new NativeHashSet<int>(tryActivate.Length, Allocator.Temp);

                // 먼저 모든 구조적 변경을 수행 (버퍼 추가 등)
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

                    // 유효하지 않은 요청은 제거 목록에 추가
                    if (abilityId == 0)
                    {
                        handlesToRemove.Add(req.Handle.Value);
                        continue;
                    }

                    if (!AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var def))
                    {
                        handlesToRemove.Add(req.Handle.Value);
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

                    // 처리 완료된 요청도 제거 목록에 추가
                    handlesToRemove.Add(req.Handle.Value);
                }

                // 구조적 변경 후 버퍼를 다시 얻어서 요청 제거
                tryActivate = em.GetBuffer<TryActivateAbilityRequest>(entity);
                for (int i = tryActivate.Length - 1; i >= 0; i--)
                {
                    var req = tryActivate[i];
                    if (handlesToRemove.Contains(req.Handle.Value))
                    {
                        tryActivate.RemoveAt(i);
                    }
                }
            }
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

