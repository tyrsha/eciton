using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Fireball 투사체의 비행/충돌(시간 기반 스텁)과 폭발 시 효과 적용을 처리.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectRequestSystem))]
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
                    // 충돌/폭발: 타겟에게 즉시 데미지 + 화상 DoT 적용
                    if (!em.HasBuffer<ApplyEffectRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectRequest>(projectile.Target);

                    // 즉시 데미지 (Health - ImpactDamage)
                    effects.Add(new ApplyEffectRequest
                    {
                        Spec = new EffectSpec
                        {
                            EffectId = ExampleIds.Effect_FireballImpactDamage,
                            Level = 1,
                            Source = projectile.Source,
                            Target = projectile.Target,
                            Duration = 0f,
                            IsPermanent = true,
                            IsPeriodic = false,
                            Period = 0f,
                            GrantedTag = GameplayTag.Invalid,
                            Modifier = new AttributeModifier
                            {
                                Attribute = AttributeId.Health,
                                Op = AttributeModOp.Add,
                                Magnitude = -projectile.ImpactDamage,
                            }
                        }
                    });

                    // 화상 DoT (Duration 동안 Period마다 Health 감소)
                    float period = projectile.BurnTickPeriod <= 0f ? 1f : projectile.BurnTickPeriod;
                    float dmgPerTick = projectile.BurnDamagePerSecond * period;

                    effects.Add(new ApplyEffectRequest
                    {
                        Spec = new EffectSpec
                        {
                            EffectId = ExampleIds.Effect_BurnDot,
                            Level = 1,
                            Source = projectile.Source,
                            Target = projectile.Target,
                            Duration = projectile.BurnDuration,
                            IsPermanent = false,
                            IsPeriodic = true,
                            Period = period,
                            GrantedTag = new GameplayTag { Value = ExampleIds.Tag_Burning },
                            Modifier = new AttributeModifier
                            {
                                Attribute = AttributeId.Health,
                                Op = AttributeModOp.Add,
                                Magnitude = -dmgPerTick,
                            }
                        }
                    });
                }

                // 투사체 제거
                em.DestroyEntity(entity);
            }).Run();
        }
    }
}

