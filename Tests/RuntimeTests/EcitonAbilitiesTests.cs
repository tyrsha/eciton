using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonAbilitiesTests
    {
        private World _world;
        private EntityManager _em;

        private SystemHandle _commonAbilities;
        private SystemHandle _fireballAbility;
        private SystemHandle _grant;
        private SystemHandle _effectFromDb;
        private SystemHandle _execute;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonAbilityTests");
            _em = _world.EntityManager;

            CreateTestDatabaseSingleton();

            _grant = _world.CreateSystem<AbilityGrantSystem>();
            _effectFromDb = _world.CreateSystem<EffectFromDatabaseSystem>();
            _execute = _world.CreateSystem<AbilityExecutionSystem>();
            _commonAbilities = _world.CreateSystem<CommonAbilitySystems>();
            _fireballAbility = _world.CreateSystem<FireballAbilitySystem>();
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

            // Fireball 부??
            var handle = new AbilityHandle { Value = 99 };
            _em.GetBuffer<GrantedAbility>(actor1).Add(new GrantedAbility
            {
                Handle = handle,
                AbilityId = CommonIds.Ability_Fireball,
                Level = 1,
                Source = actor1
            });

            // ?�턴 ?�그 부??
            _em.GetBuffer<GameplayTagElement>(actor1).Add(new GameplayTagElement
            {
                Tag = new GameplayTag { Value = CommonIds.Tag_Stunned }
            });

            _em.GetBuffer<TryActivateAbilityRequest>(actor1).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = actor2
            });

            _execute.Update(_world.Unmanaged);

            // ?�청?� ?�비?�고, ?�사체는 ?�성?��? ?�아????
            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(actor1).Length);

            int projectileCount = 0;
            using (var q = _em.CreateEntityQuery(typeof(AbilityProjectile)))
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
            _grant.Update(_world.Unmanaged);
            var handle = _em.GetBuffer<GrantedAbility>(actor)[0].Handle;

            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = Entity.Null
            });

            _commonAbilities.Update(_world.Unmanaged);
            _execute.Update(_world.Unmanaged);

            // Heal?� ApplyEffectRequest가 target(=self)???�인??
            Assert.AreEqual(1, _em.GetBuffer<ApplyEffectByIdRequest>(actor).Length);

            // DB 변???�스?�을 ?�리�?ApplyEffectRequest�?변?�된??
            _effectFromDb.Update(_world.Unmanaged);
            Assert.AreEqual(1, _em.GetBuffer<ApplyEffectRequest>(actor).Length);
        }

        [Test]
        public void Ability_gate_blocks_activation_when_on_cooldown_or_insufficient_mana()
        {
            var gate = _world.CreateSystem<AbilityActivationGateSystem>();

            var actor = CreateAscActor(health: 100f);
            var target = CreateAscActor(health: 100f);

            // ?�력 부??DB): 쿨다??10�? 마나 코스??20
            _em.GetBuffer<GrantAbilityRequest>(actor).Add(new GrantAbilityRequest { AbilityId = 20, Level = 1, Source = actor });
            _grant.Update(_world.Unmanaged);
            var handle = _em.GetBuffer<GrantedAbility>(actor)[0].Handle;

            // 쿨다??중으�??�팅
            var grantedBuffer1 = _em.GetBuffer<GrantedAbility>(actor);
            var ga0 = grantedBuffer1[0];
            ga0.CooldownRemaining = 5f;
            grantedBuffer1[0] = ga0;

            // 마나 부�?0) + 쿨다??�? ?�청?� ?�거?�어????
            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = target,
                TargetData = new TargetData { Target = target }
            });

            gate.Update(_world.Unmanaged);
            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(actor).Length);

            // 쿨다??0, 마나 충분?�면 ?�과 + 코스??차감 + 쿨다???�작
            var grantedBuffer2 = _em.GetBuffer<GrantedAbility>(actor);
            var ga = grantedBuffer2[0];
            ga.CooldownRemaining = 0f;
            grantedBuffer2[0] = ga;

            var attrs = _em.GetComponentData<AttributeData>(actor);
            attrs.Mana = 50f;
            _em.SetComponentData(actor, attrs);

            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = target,
                TargetData = new TargetData { Target = target }
            });

            gate.Update(_world.Unmanaged);
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
            _em.AddBuffer<PendingGameplayEvent>(e);

            _em.AddBuffer<GameplayTagElement>(e);
            _em.AddBuffer<AddGameplayTagRequest>(e);
            _em.AddBuffer<RemoveGameplayTagRequest>(e);
            return e;
        }

        private void CreateTestDatabaseSingleton()
        {
            var builder = new BlobBuilder(AllocatorManager.Temp);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, 2);
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
                TagRequirements = default,
                CooldownEffectId = 2001,
                CooldownTag = new GameplayTag { Value = 3001 },
            };

            // 게이???�스?�용 AbilityId=20 (쿨다??remaining 방식)
            abilities[1] = new AbilityDefinition
            {
                AbilityId = 20,
                ExecutionType = AbilityExecutionType.ApplyEffectToTarget,
                CooldownDuration = 10f,
                ManaCost = 20f,
                PrimaryEffectId = CommonIds.Effect_HealInstant,
                SecondaryEffectId = 0,
                CleanseTag = GameplayTag.Invalid,
                ProjectileFlightTime = 0f,
                TagRequirements = default,
                CooldownEffectId = 0,
                CooldownTag = GameplayTag.Invalid,
            };

            var effects = builder.Allocate(ref root.Effects, 2);
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
                BlockedByTag = GameplayTag.Invalid,
            };
            var m0 = builder.Allocate(ref effects[0].Modifiers, 1);
            m0[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = 25f };

            // Heal cooldown effect: duration 2s, grants cooldown tag, no modifiers
            effects[1] = new EffectDefinition
            {
                EffectId = 2001,
                Duration = 2f,
                IsPermanent = false,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = new GameplayTag { Value = 3001 },
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.RefreshDuration,
                MaxStacks = 1,
                BlockedByTag = GameplayTag.Invalid,
            };
            builder.Allocate(ref effects[1].Modifiers, 0);

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(AllocatorManager.Persistent);
            builder.Dispose();

            var dbEntity = _em.CreateEntity();
            _em.AddComponentData(dbEntity, new AbilityEffectDatabase { Blob = blob });
        }

        [Test]
        public void Ability_input_binding_and_cooldown_effect_tag_blocks_second_activation()
        {
            var input = _world.CreateSystem<AbilityInputSystem>();
            var gate = _world.CreateSystem<AbilityActivationGateSystem>();
            var effectReq = _world.CreateSystem<EffectRequestSystem>();
            var tagSys = _world.CreateSystem<GameplayTagSystem>();

            var actor = CreateAscActor(health: 100f);

            // Heal 부??GrantAbilityRequest -> GrantedAbility ?�성)
            _em.GetBuffer<GrantAbilityRequest>(actor).Add(new GrantAbilityRequest { AbilityId = CommonIds.Ability_Heal, Level = 1, Source = actor });
            _grant.Update(_world.Unmanaged);
            var handle = _em.GetBuffer<GrantedAbility>(actor)[0].Handle;

            // ?�롯 바인??
            _em.AddBuffer<AbilityInputBinding>(actor).Add(new AbilityInputBinding { Slot = AbilityInputSlot.Slot1, Handle = handle });
            _em.AddBuffer<PressAbilityInputRequest>(actor);

            // 1???�력 -> ?�성???�공 -> 쿨다??effect ?�용 -> 쿨다???�그 ?��?
            _em.GetBuffer<PressAbilityInputRequest>(actor).Add(new PressAbilityInputRequest { Slot = AbilityInputSlot.Slot1, TargetData = default });
            input.Update(_world.Unmanaged);
            gate.Update(_world.Unmanaged);
            _execute.Update(_world.Unmanaged);
            _effectFromDb.Update(_world.Unmanaged);
            effectReq.Update(_world.Unmanaged);
            tagSys.Update(_world.Unmanaged);

            Assert.IsTrue(HasTag(actor, 3001));

            // 2???�력 -> 게이?�에??쿨다???�그�?차단?�어 TryActivate가 ?�거?�어????
            _em.GetBuffer<PressAbilityInputRequest>(actor).Add(new PressAbilityInputRequest { Slot = AbilityInputSlot.Slot1, TargetData = default });
            input.Update(_world.Unmanaged);
            gate.Update(_world.Unmanaged);

            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(actor).Length);
        }

        private bool HasTag(Entity e, int tagValue)
        {
            var tags = _em.GetBuffer<GameplayTagElement>(e);
            for (int i = 0; i < tags.Length; i++)
                if (tags[i].Tag.Value == tagValue)
                    return true;
            return false;
        }
    }
}

