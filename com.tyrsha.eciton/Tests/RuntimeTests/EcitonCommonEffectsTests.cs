using NUnit.Framework;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonCommonEffectsTests
    {
        private World _world;
        private EntityManager _em;

        private EffectRequestSystem _effectRequest;
        private ActiveEffectSystem _activeEffect;
        private AttributeModifierSystem _attributeMod;
        private GameplayTagSystem _tagSystem;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonTests");
            _em = _world.EntityManager;

            _effectRequest = _world.CreateSystemManaged<EffectRequestSystem>();
            _activeEffect = _world.CreateSystemManaged<ActiveEffectSystem>();
            _attributeMod = _world.CreateSystemManaged<AttributeModifierSystem>();
            _tagSystem = _world.CreateSystemManaged<GameplayTagSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void Shield_absorbs_damage_before_health()
        {
            var target = CreateTarget(health: 100f, shield: 20f);

            var apply = _em.GetBuffer<ApplyEffectRequest>(target);
            apply.Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_InstantDamage,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 0f,
                    IsPermanent = true,
                    IsPeriodic = false,
                    Period = 0f,
                    GrantedTag = GameplayTag.Invalid,
                    RevertModifierOnEnd = false,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.Health,
                        Op = AttributeModOp.Add,
                        Magnitude = -30f
                    }
                }
            });

            Tick(dt: 0f);

            var attrs = _em.GetComponentData<AttributeData>(target);
            Assert.AreEqual(0f, attrs.Shield, 0.0001f);
            Assert.AreEqual(90f, attrs.Health, 0.0001f);
        }

        [Test]
        public void Burning_tag_is_added_and_removed_on_expire()
        {
            var target = CreateTarget(health: 100f, shield: 0f);

            // Burn: 2초 동안 1초마다 -5
            _em.GetBuffer<ApplyEffectRequest>(target).Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_BurnDot,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 2f,
                    IsPermanent = false,
                    IsPeriodic = true,
                    Period = 1f,
                    GrantedTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                    RevertModifierOnEnd = false,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.Health,
                        Op = AttributeModOp.Add,
                        Magnitude = -5f
                    }
                }
            });

            // 적용 프레임: 태그 추가(틱은 아직)
            Tick(dt: 0f);
            Assert.IsTrue(HasTag(target, CommonIds.Tag_Burning));

            // 2초 경과: 2틱 적용 + 만료로 태그 제거
            Tick(dt: 1f);
            Tick(dt: 1f);

            var attrs = _em.GetComponentData<AttributeData>(target);
            Assert.AreEqual(90f, attrs.Health, 0.0001f);
            Assert.IsFalse(HasTag(target, CommonIds.Tag_Burning));
        }

        [Test]
        public void Slow_buff_reverts_on_expire()
        {
            var target = CreateTarget(health: 100f, shield: 0f, moveSpeed: 10f);

            _em.GetBuffer<ApplyEffectRequest>(target).Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_Slow,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 2f,
                    IsPermanent = false,
                    IsPeriodic = false,
                    Period = 0f,
                    GrantedTag = new GameplayTag { Value = CommonIds.Tag_Slowed },
                    RevertModifierOnEnd = true,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.MoveSpeed,
                        Op = AttributeModOp.Multiply,
                        Magnitude = 0.5f
                    }
                }
            });

            Tick(dt: 0f);
            Assert.AreEqual(5f, _em.GetComponentData<AttributeData>(target).MoveSpeed, 0.0001f);
            Assert.IsTrue(HasTag(target, CommonIds.Tag_Slowed));

            Tick(dt: 1f);
            Tick(dt: 1f);

            Assert.AreEqual(10f, _em.GetComponentData<AttributeData>(target).MoveSpeed, 0.0001f);
            Assert.IsFalse(HasTag(target, CommonIds.Tag_Slowed));
        }

        [Test]
        public void Cleanse_removes_burning_effects_by_tag()
        {
            var target = CreateTarget(health: 100f, shield: 0f);

            // Burn 5초, 1초마다 -5
            _em.GetBuffer<ApplyEffectRequest>(target).Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_BurnDot,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 5f,
                    IsPermanent = false,
                    IsPeriodic = true,
                    Period = 1f,
                    GrantedTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                    RevertModifierOnEnd = false,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.Health,
                        Op = AttributeModOp.Add,
                        Magnitude = -5f
                    }
                }
            });

            Tick(dt: 0f);
            Assert.IsTrue(HasTag(target, CommonIds.Tag_Burning));

            // 1틱
            Tick(dt: 1f);
            Assert.AreEqual(95f, _em.GetComponentData<AttributeData>(target).Health, 0.0001f);

            // Cleanse: 태그 기반 효과 제거
            _em.GetBuffer<RemoveEffectsWithTagRequest>(target).Add(new RemoveEffectsWithTagRequest
            {
                Tag = new GameplayTag { Value = CommonIds.Tag_Burning }
            });
            Tick(dt: 0f);
            Assert.IsFalse(HasTag(target, CommonIds.Tag_Burning));

            // 이후 시간이 흘러도 추가 틱이 없어야 함
            Tick(dt: 2f);
            Assert.AreEqual(95f, _em.GetComponentData<AttributeData>(target).Health, 0.0001f);
        }

        [Test]
        public void Effect_stacking_refresh_duration_resets_remaining_time()
        {
            var target = CreateTarget(health: 100f, shield: 0f);

            // 2초짜리 burning(태그 기준으로 merge)
            _em.GetBuffer<ApplyEffectRequest>(target).Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_BurnDot,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 2f,
                    IsPermanent = false,
                    IsPeriodic = true,
                    Period = 1f,
                    GrantedTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                    RevertModifierOnEnd = false,
                    StackingPolicy = EffectStackingPolicy.RefreshDuration,
                    MaxStacks = 1,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.Health,
                        Op = AttributeModOp.Add,
                        Magnitude = -1f
                    }
                }
            });

            Tick(dt: 0f);
            Assert.AreEqual(1, _em.GetBuffer<ActiveEffect>(target).Length);

            // 1초 진행 후 다시 적용 => remainingTime이 2로 리셋
            Tick(dt: 1f);
            var before = _em.GetBuffer<ActiveEffect>(target)[0].RemainingTime;

            _em.GetBuffer<ApplyEffectRequest>(target).Add(new ApplyEffectRequest
            {
                Spec = new EffectSpec
                {
                    EffectId = CommonIds.Effect_BurnDot,
                    Level = 1,
                    Source = target,
                    Target = target,
                    Duration = 2f,
                    IsPermanent = false,
                    IsPeriodic = true,
                    Period = 1f,
                    GrantedTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                    RevertModifierOnEnd = false,
                    StackingPolicy = EffectStackingPolicy.RefreshDuration,
                    MaxStacks = 1,
                    Modifier = new AttributeModifier
                    {
                        Attribute = AttributeId.Health,
                        Op = AttributeModOp.Add,
                        Magnitude = -1f
                    }
                }
            });

            Tick(dt: 0f);
            var after = _em.GetBuffer<ActiveEffect>(target)[0].RemainingTime;
            Assert.Less(before, after);
        }

        private Entity CreateTarget(float health, float shield, float moveSpeed = 0f)
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, new AttributeData
            {
                Health = health,
                Mana = 0f,
                Strength = 0f,
                Agility = 0f,
                Shield = shield,
                MoveSpeed = moveSpeed
            });

            _em.AddBuffer<ApplyEffectRequest>(e);
            _em.AddBuffer<RemoveEffectRequest>(e);
            _em.AddBuffer<RemoveEffectsWithTagRequest>(e);
            _em.AddBuffer<ActiveEffect>(e);
            _em.AddBuffer<ApplyAttributeModifierRequest>(e);

            _em.AddBuffer<GameplayTagElement>(e);
            _em.AddBuffer<AddGameplayTagRequest>(e);
            _em.AddBuffer<RemoveGameplayTagRequest>(e);
            return e;
        }

        private bool HasTag(Entity e, int tagValue)
        {
            var tags = _em.GetBuffer<GameplayTagElement>(e);
            for (int i = 0; i < tags.Length; i++)
                if (tags[i].Tag.Value == tagValue)
                    return true;
            return false;
        }

        private void Tick(float dt)
        {
            // Entities 버전에 따라 SetTime API가 다를 수 있어, 없으면 그냥 시스템 Update만 호출되도록 한다.
            TrySetWorldTime(dt);

            // 의도한 파이프라인 순서
            _effectRequest.Update();
            _activeEffect.Update();
            _attributeMod.Update();
            _tagSystem.Update();
        }

        private void TrySetWorldTime(float dt)
        {
            // Entities 1.0+: World.SetTime(TimeData)
            // 스텁: 컴파일 호환성을 위해 리플렉션 사용.
            var worldType = typeof(World);
            var setTime = worldType.GetMethod("SetTime", new[] { typeof(TimeData) });
            if (setTime == null)
                return;

            var current = _world.Time;
            var next = new TimeData(current.ElapsedTime + dt, dt);
            setTime.Invoke(_world, new object[] { next });
        }
    }
}

