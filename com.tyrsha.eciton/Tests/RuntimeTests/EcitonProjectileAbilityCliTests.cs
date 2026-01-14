using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonProjectileAbilityCliTests
    {
        private World _world;
        private EntityManager _em;

        private AbilityGrantSystem _grant;
        private AbilityActivationGateSystem _gate;
        private AbilityExecutionSystem _exec;
        private AbilityProjectileSystem _projectile;
        private EffectFromDatabaseSystem _effectFromDb;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonProjectileAbilityTests");
            _em = _world.EntityManager;

            CreateTestDatabaseSingleton();

            _grant = _world.CreateSystemManaged<AbilityGrantSystem>();
            _gate = _world.CreateSystemManaged<AbilityActivationGateSystem>();
            _exec = _world.CreateSystemManaged<AbilityExecutionSystem>();
            _projectile = _world.CreateSystemManaged<AbilityProjectileSystem>();
            _effectFromDb = _world.CreateSystemManaged<EffectFromDatabaseSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void Projectile_execution_spawns_projectile_and_enqueues_effects_on_hit()
        {
            var caster = CreateAscActor(mana: 100f);
            var target = CreateAscActor(mana: 0f);

            // Fireball 부여
            _em.GetBuffer<GrantAbilityRequest>(caster).Add(new GrantAbilityRequest { AbilityId = 10, Level = 1, Source = caster });
            _grant.Update();
            var handle = _em.GetBuffer<GrantedAbility>(caster)[0].Handle;

            // 시전 요청
            _em.GetBuffer<TryActivateAbilityRequest>(caster).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = target,
                TargetData = new TargetData { Target = target }
            });

            // 게이트 통과 후 실행 => 투사체 생성
            _gate.Update();
            _exec.Update();

            int projectileCount;
            using (var q = _em.CreateEntityQuery(typeof(AbilityProjectile)))
                projectileCount = q.CalculateEntityCount();
            Assert.AreEqual(1, projectileCount);

            // 충분한 dt로 투사체 "히트" 처리
            SetWorldTime(dt: 1f);
            _projectile.Update();

            // 타겟에 effectById 요청이 들어가야 함(2개)
            Assert.AreEqual(2, _em.GetBuffer<ApplyEffectByIdRequest>(target).Length);

            // DB 변환 시스템이 ApplyEffectRequest로 변환
            _effectFromDb.Update();
            Assert.AreEqual(2, _em.GetBuffer<ApplyEffectRequest>(target).Length);
        }

        private Entity CreateAscActor(float mana)
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, new AbilitySystemComponent { Owner = e, Avatar = e });
            _em.AddComponentData(e, new AttributeData
            {
                Health = 100f,
                Mana = mana,
                Strength = 0f,
                Agility = 0f,
                Shield = 0f,
                MoveSpeed = 0f
            });

            _em.AddBuffer<GrantedAbility>(e);
            _em.AddBuffer<GrantAbilityRequest>(e);
            _em.AddBuffer<TryActivateAbilityRequest>(e);
            _em.AddBuffer<CancelAbilityRequest>(e);

            _em.AddBuffer<ApplyEffectByIdRequest>(e);
            _em.AddBuffer<ApplyEffectRequest>(e);
            _em.AddBuffer<RemoveEffectRequest>(e);
            _em.AddBuffer<RemoveEffectsWithTagRequest>(e);
            _em.AddBuffer<ActiveEffect>(e);
            _em.AddBuffer<ApplyAttributeModifierRequest>(e);
            _em.AddBuffer<PendingGameplayEvent>(e);

            _em.AddBuffer<GameplayTagElement>(e);
            _em.AddBuffer<AddGameplayTagRequest>(e);
            _em.AddBuffer<RemoveGameplayTagRequest>(e);
            return e;
        }

        private void CreateTestDatabaseSingleton()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            // AbilityId=10: projectile, flight 0.1, apply effects 100 and 101
            var abilities = builder.Allocate(ref root.Abilities, 1);
            abilities[0] = new AbilityDefinition
            {
                AbilityId = 10,
                ExecutionType = AbilityExecutionType.SpawnProjectileAndApplyEffectsOnHit,
                CooldownDuration = 0f,
                ManaCost = 0f,
                TagRequirements = default,
                ProjectileFlightTime = 0.1f,
                PrimaryEffectId = 100,
                SecondaryEffectId = 101,
                CleanseTag = GameplayTag.Invalid,
                CooldownEffectId = 0,
                CooldownTag = GameplayTag.Invalid,
            };

            var effects = builder.Allocate(ref root.Effects, 2);
            effects[0] = new EffectDefinition
            {
                EffectId = 100,
                Duration = 0f,
                IsPermanent = true,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = GameplayTag.Invalid,
                BlockedByTag = GameplayTag.Invalid,
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.None,
                MaxStacks = 1,
            };
            var m0 = builder.Allocate(ref effects[0].Modifiers, 1);
            m0[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = -1f, DamageType = DamageType.Fire };

            effects[1] = new EffectDefinition
            {
                EffectId = 101,
                Duration = 0f,
                IsPermanent = true,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = GameplayTag.Invalid,
                BlockedByTag = GameplayTag.Invalid,
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.None,
                MaxStacks = 1,
            };
            var m1 = builder.Allocate(ref effects[1].Modifiers, 1);
            m1[0] = new AttributeModifier { Attribute = AttributeId.Mana, Op = AttributeModOp.Add, Magnitude = -2f };

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(Allocator.Persistent);
            builder.Dispose();

            var dbEntity = _em.CreateEntity();
            _em.AddComponentData(dbEntity, new AbilityEffectDatabase { Blob = blob });
        }

        private void SetWorldTime(float dt)
        {
            // Entities 1.0+: World.SetTime(TimeData)
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

