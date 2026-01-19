using Unity.Entities;
using Unity.Collections;

namespace Tyrsha.Eciton
{
    /// <summary>StunBolt 투사체 비행/충돌(시간 기반 스텁) 및 스턴 효과 적용.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectFromDatabaseSystem))]
    public partial struct StunBoltProjectileSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var em = state.EntityManager;

            var query = state.GetEntityQuery(ComponentType.ReadWrite<StunBoltProjectile>());
            using var entities = query.ToEntityArray(Allocator.Temp);
            using var projectiles = query.ToComponentDataArray<StunBoltProjectile>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var projectile = projectiles[i];

                projectile.RemainingFlightTime -= dt;
                if (projectile.RemainingFlightTime > 0f)
                {
                    em.SetComponentData(entity, projectile);
                    continue;
                }

                if (projectile.Target != Entity.Null && em.Exists(projectile.Target))
                {
                    if (!em.HasBuffer<ApplyEffectByIdRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectByIdRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectByIdRequest>(projectile.Target);
                    if (projectile.EffectId != 0)
                        effects.Add(new ApplyEffectByIdRequest { EffectId = projectile.EffectId, Level = 1, Source = projectile.Source, Target = projectile.Target });
                }

                em.DestroyEntity(entity);
            }
        }
    }
}

