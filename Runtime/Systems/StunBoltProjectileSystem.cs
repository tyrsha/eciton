using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>StunBolt 투사체 비행/충돌(시간 기반 스텁) 및 스턴 효과 적용.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectFromDatabaseSystem))]
    public class StunBoltProjectileSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var em = EntityManager;

            Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, ref StunBoltProjectile projectile) =>
            {
                projectile.RemainingFlightTime -= dt;
                if (projectile.RemainingFlightTime > 0f)
                    return;

                if (projectile.Target != Entity.Null && em.Exists(projectile.Target))
                {
                    if (!em.HasBuffer<ApplyEffectByIdRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectByIdRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectByIdRequest>(projectile.Target);
                    if (projectile.EffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.EffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                }

                em.DestroyEntity(entity);
            }).Run();
        }
    }
}

