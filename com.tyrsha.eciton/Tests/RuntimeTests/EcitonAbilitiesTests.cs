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

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonAbilityTests");
            _em = _world.EntityManager;

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

            var handle = new AbilityHandle { Value = 1 };
            _em.GetBuffer<GrantedAbility>(actor).Add(new GrantedAbility
            {
                Handle = handle,
                AbilityId = CommonIds.Ability_Heal,
                Level = 1,
                Source = actor
            });

            _em.GetBuffer<TryActivateAbilityRequest>(actor).Add(new TryActivateAbilityRequest
            {
                Handle = handle,
                Target = Entity.Null
            });

            _commonAbilities.Update();

            // Heal은 ApplyEffectRequest가 target(=self)에 쌓인다.
            Assert.AreEqual(1, _em.GetBuffer<ApplyEffectRequest>(actor).Length);
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
            _em.AddBuffer<TryActivateAbilityRequest>(e);
            _em.AddBuffer<CancelAbilityRequest>(e);

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
    }
}

