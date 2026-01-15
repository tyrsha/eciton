using NUnit.Framework;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonAbilitiesTests
    {
        private World _world;
        private EntityManager _em;

        private CommonAbilitySystems _commonAbilities;
        private FireballAbilitySystem _fireballAbility;
        private AbilityGrantSystem _grant;
        private EffectFromDatabaseSystem _effectFromDb;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonAbilityTests");
            _em = _world.EntityManager;

            CreateTestDatabaseSingleton();

            _grant = _world.CreateSystemManaged<AbilityGrantSystem>();
            _effectFromDb = _world.CreateSystemManaged<EffectFromDatabaseSystem>();
            _commonAbilities = _world.CreateSystemManaged<CommonAbilitySystems>();
            _fireballAbility = _world.CreateSystemManaged<FireballAbilitySystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void Stunned_actor_cannot_activate_fireball()
        {
            var actor1 = CreateAscActor();
            var actor2 = CreateAscActor();

            // Fireball 부여
            var handle = new AbilityHandle { Value = 99 };
            _em.GetBuffer<GrantedAbility>(actor1).Add(new GrantedAbility
            {
                Handle = handle,
                AbilityId = CommonIds.Ability_Fireball,
                Level = 1,
                Source = actor1
            });

            // 스턴 태그 부여
            _em.GetBuffer<GameplayTagElement>(actor1).Add(new GameplayTagElement
            {
                Tag = new GameplayTag { Value = CommonIds.Tag_Stunned }
            });

            _em.GetBuffer<TryActivateAbilityRequest>(actor1).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = actor2
            });

            _fireballAbility.Update();

            // 요청은 소비되고, 투사체는 생성되지 않아야 함
            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(actor1).Length);

            int projectileCount = 0;
            using (var q = _em.CreateEntityQuery(typeof(FireballProjectile)))
                projectileCount = q.CalculateEntityCount();
            Assert.AreEqual(0, projectileCount);
        }

        [Test]
        public void Heal_ability_adds_heal_effect_request()
        {
            var actor = CreateAscActor(health: 50f);

            _em.GetBuffer<GrantAbilityRequest>(actor).Add(new GrantAbilityRequest
            {
                AbilityId = CommonIds.Ability_Heal,
                Level = 1,
                Source = actor
            });
            _grant.Update();
            var handle = _em.GetBuffer<GrantedAbility>(actor)[0].Handle;

            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = Entity.Null
            });

            _commonAbilities.Update();

            // Heal은 ApplyEffectRequest가 target(=self)에 쌓인다.
            Assert.AreEqual(1, _em.GetBuffer<ApplyEffectByIdRequest>(actor).Length);

            // DB 변환 시스템을 돌리면 ApplyEffectRequest로 변환된다.
            _effectFromDb.Update();
            Assert.AreEqual(1, _em.GetBuffer<ApplyEffectRequest>(actor).Length);
        }

        [Test]
        public void Ability_gate_blocks_activation_when_on_cooldown_or_insufficient_mana()
        {
            var world = _world;
            var gate = world.CreateSystemManaged<AbilityActivationGateSystem>();

            var actor = CreateAscActor(health: 100f);
            var target = CreateAscActor(health: 100f);

            // 능력 부여: 쿨다운 10초, 마나 코스트 20
            var handle = new AbilityHandle { Value = 7, Version = 1 };
            _em.GetBuffer<GrantedAbility>(actor).Add(new GrantedAbility
            {
                Handle = handle,
                AbilityId = CommonIds.Ability_Heal,
                Level = 1,
                Source = actor,
                CooldownDuration = 10f,
                CooldownRemaining = 5f,
                ManaCost = 20f,
                TagRequirements = default
            });

            // 마나 부족(0) + 쿨다운 중: 요청은 제거되어야 함
            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = target,
                TargetData = new TargetData { Target = target }
            });

            gate.Update();
            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(actor).Length);

            // 쿨다운 0, 마나 충분이면 통과 + 코스트 차감 + 쿨다운 시작
            var ga = _em.GetBuffer<GrantedAbility>(actor)[0];
            ga.CooldownRemaining = 0f;
            _em.GetBuffer<GrantedAbility>(actor)[0] = ga;

            var attrs = _em.GetComponentData<AttributeData>(actor);
            attrs.Mana = 50f;
            _em.SetComponentData(actor, attrs);

            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = target,
                TargetData = new TargetData { Target = target }
            });

            gate.Update();
            Assert.AreEqual(1, _em.GetBuffer<TryActivateAbilityRequest>(actor).Length);

            var after = _em.GetComponentData<AttributeData>(actor);
            Assert.AreEqual(30f, after.Mana, 0.0001f);
            Assert.Greater(_em.GetBuffer<GrantedAbility>(actor)[0].CooldownRemaining, 0f);
        }

        private Entity CreateAscActor(float health = 100f)
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, new AbilitySystemComponent { Owner = e, Avatar = e });
            _em.AddComponentData(e, new AttributeData
            {
                Health = health,
                Mana = 0f,
                Strength = 0f,
                Agility = 0f,
                Shield = 0f,
                MoveSpeed = 0f
            });

            _em.AddBuffer<GrantedAbility>(e);
            _em.AddBuffer<GrantAbilityRequest>(e);
            _em.AddBuffer<TryActivateAbilityRequest>(e);
            _em.AddBuffer<CancelAbilityRequest>(e);

            _em.AddBuffer<ApplyEffectRequest>(e);
            _em.AddBuffer<ApplyEffectByIdRequest>(e);
            _em.AddBuffer<RemoveEffectRequest>(e);
            _em.AddBuffer<RemoveEffectsWithTagRequest>(e);
            _em.AddBuffer<ActiveEffect>(e);
            _em.AddBuffer<ApplyAttributeModifierRequest>(e);

            _em.AddBuffer<GameplayTagElement>(e);
            _em.AddBuffer<AddGameplayTagRequest>(e);
            _em.AddBuffer<RemoveGameplayTagRequest>(e);
            return e;
        }

        private void CreateTestDatabaseSingleton()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, 1);
            abilities[0] = new AbilityDefinition
            {
                AbilityId = CommonIds.Ability_Heal,
                ExecutionType = AbilityExecutionType.ApplyEffectToTarget,
                CooldownDuration = 2f,
                ManaCost = 0f,
                PrimaryEffectId = CommonIds.Effect_HealInstant,
                SecondaryEffectId = 0,
                CleanseTag = GameplayTag.Invalid,
                ProjectileFlightTime = 0f,
                TagRequirements = default
            };

            var effects = builder.Allocate(ref root.Effects, 1);
            effects[0] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_HealInstant,
                Duration = 0f,
                IsPermanent = true,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = GameplayTag.Invalid,
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.None,
                MaxStacks = 1,
                Modifier = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = 25f }
            };

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(Allocator.Persistent);
            builder.Dispose();

            var dbEntity = _em.CreateEntity();
            _em.AddComponentData(dbEntity, new AbilityEffectDatabase { Blob = blob });
        }
    }
}

