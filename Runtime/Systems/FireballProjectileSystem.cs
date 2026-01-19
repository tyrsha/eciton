using Unity.Entities;
using Unity.Collections;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Fireball 투사체의 비행/충돌(시간 기반 스텁)과 폭발 시 효과 적용을 처리.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectFromDatabaseSystem))]
    public partial struct FireballProjectileSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var em = state.EntityManager;

            var query = state.GetEntityQuery(ComponentType.ReadWrite<FireballProjectile>());
            using var entities = query.ToEntityArray(Allocator.Temp);
            using var projectiles = query.ToComponentDataArray<FireballProjectile>(Allocator.Temp);

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
            }
        }
    }
}

