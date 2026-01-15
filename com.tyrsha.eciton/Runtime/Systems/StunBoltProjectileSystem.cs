using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>StunBolt 투사체 비행/충돌(시간 기반 스텁) 및 스턴 효과 적용.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectRequestSystem))]
    public class StunBoltProjectileSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            var em = EntityManager;

            Entities.WithoutBurst().ForEach((Entity entity, ref StunBoltProjectile projectile) =>
            {
                projectile.RemainingFlightTime -= dt;
                if (projectile.RemainingFlightTime > 0f)
                    return;

                if (projectile.Target != Entity.Null && em.Exists(projectile.Target))
                {
                    if (!em.HasBuffer<ApplyEffectRequest>(projectile.Target))
                        em.AddBuffer<ApplyEffectRequest>(projectile.Target);

                    var effects = em.GetBuffer<ApplyEffectRequest>(projectile.Target);
                    effects.Add(new ApplyEffectRequest
                    {
                        Spec = new EffectSpec
                        {
                            EffectId = CommonIds.Effect_Stun,
                            Level = 1,
                            Source = projectile.Source,
                            Target = projectile.Target,
                            Duration = projectile.StunDuration,
                            IsPermanent = false,
                            IsPeriodic = false,
                            Period = 0f,
                            GrantedTag = new GameplayTag { Value = CommonIds.Tag_Stunned },
                            RevertModifierOnEnd = false,
                            // 스텁: 태그 기반 효과이므로 modifier는 no-op
                            Modifier = new AttributeModifier
                            {
                                Attribute = AttributeId.Health,
                                Op = AttributeModOp.Add,
                                Magnitude = 0f,
                            }
                        }
                    });
                }

                em.DestroyEntity(entity);
            }).Run();
        }
    }
}

