using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// AbilityProjectile 비행/충돌(시간 기반 스텁) 처리 후 DB 기반 Effect 적용 요청을 발행.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectFromDatabaseSystem))]
    public class AbilityProjectileSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var em = EntityManager;

            Entities.WithoutBurst().ForEach((Entity entity, ref AbilityProjectile projectile) =>
            {
                projectile.RemainingFlightTime -= dt;
                if (projectile.RemainingFlightTime > 0f)
                    return;

                if (projectile.Target != Entity.Null && em.Exists(projectile.Target))
                {
                    if (!em.HasBuffer<ApplyEffectByIdRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectByIdRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectByIdRequest>(projectile.Target);
                    if (projectile.PrimaryEffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.PrimaryEffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                    if (projectile.SecondaryEffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.SecondaryEffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                }

                em.DestroyEntity(entity);
            }).Run();
        }
    }
}

