using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Fireball 투사체의 비행/충돌(시간 기반 스텁)과 폭발 시 효과 적용을 처리.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectFromDatabaseSystem))]
    public class FireballProjectileSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var em = EntityManager;

            Entities.WithoutBurst().ForEach((Entity entity, ref FireballProjectile projectile) =>
            {
                projectile.RemainingFlightTime -= dt;
                if (projectile.RemainingFlightTime > 0f)
                    return;

                if (projectile.Target != Entity.Null && em.Exists(projectile.Target))
                {
                    // 충돌/폭발: DB 정의된 Effect를 ID로 적용
                    if (!em.HasBuffer<ApplyEffectByIdRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectByIdRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectByIdRequest>(projectile.Target);
                    if (projectile.PrimaryEffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.PrimaryEffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                    if (projectile.SecondaryEffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.SecondaryEffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                }

                // 투사체 제거
                em.DestroyEntity(entity);
            }).Run();
        }
    }
}

