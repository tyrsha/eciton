using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Fireball 능력 활성화 요청을 처리해 투사체를 스폰하는 예제 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityRequestSystem))]
    [UpdateAfter(typeof(AbilityExecutionSystem))]
    public class FireballAbilitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = EntityManager;

            // 스텁 예제이므로 main thread에서 처리(간단/명확성 우선).
            Entities.WithoutBurst().WithStructuralChanges().ForEach((
                Entity entity,
                in AbilitySystemComponent asc,
                DynamicBuffer<GameplayTagElement> tags,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate) =>
            {
                _ = asc;

                // 스턴이면 발사 불가(요청은 실패로 소비)
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

                    if (abilityId != ExampleIds.Ability_Fireball)
                        continue;

                    if (!AbilityEffectDatabaseLookup.TryGetAbility(db, abilityId, out var def))
                        continue;

                    // Fireball 투사체 스폰
                    var projectile = em.CreateEntity();
                    var target = request.Target != Entity.Null ? request.Target : request.TargetData.Target;
                    em.AddComponentData(projectile, new FireballProjectile
                    {
                        Source = entity,
                        Target = target,
                        RemainingFlightTime = def.ProjectileFlightTime,
                        PrimaryEffectId = def.PrimaryEffectId,
                        SecondaryEffectId = def.SecondaryEffectId,
                    });

                    // 요청 소비
                    tryActivate.RemoveAt(i);
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
    }
}

