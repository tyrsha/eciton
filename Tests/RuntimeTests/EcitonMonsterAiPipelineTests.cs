using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonMonsterAiPipelineTests
    {
        private World _world;
        private EntityManager _em;

        private AbilityGrantSystem _grant;
        private AbilityInputAutoBindSystem _autoBind;
        private BehaviorTreeTickSystem _bt;
        private AbilityInputSystem _input;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonMonsterAiPipelineTests");
            _em = _world.EntityManager;

            CreateTestDatabaseSingleton();

            _grant = _world.CreateSystemManaged<AbilityGrantSystem>();
            _autoBind = _world.CreateSystemManaged<AbilityInputAutoBindSystem>();
            _bt = _world.CreateSystemManaged<BehaviorTreeTickSystem>();
            _input = _world.CreateSystemManaged<AbilityInputSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void AutoBind_then_BT_press_then_InputSystem_creates_TryActivate()
        {
            // BT: Press Slot1
            var btBlob = BuildSinglePressTree();

            var monster = _em.CreateEntity();
            _em.AddComponentData(monster, new BehaviorTreeAgent { Tree = btBlob });
            _em.AddComponentData(monster, new BehaviorTreeBlackboard { Target = new Entity { Index = 10, Version = 1 }, TargetInRange = 1, DesiredStoppingDistance = 1f });
            _em.AddComponentData(monster, new BehaviorTreeLastResult { Status = BtStatus.Failure, LastNodeIndex = -1 });

            _em.AddBuffer<GrantedAbility>(monster);
            _em.AddBuffer<GrantAbilityRequest>(monster);
            _em.AddBuffer<AbilityInputBindingByAbilityId>(monster);
            _em.AddBuffer<AbilityInputBinding>(monster);
            _em.AddBuffer<PressAbilityInputRequest>(monster);
            _em.AddBuffer<TryActivateAbilityRequest>(monster);

            // 원하는 바인딩: Slot1 -> AbilityId 42
            _em.GetBuffer<AbilityInputBindingByAbilityId>(monster).Add(new AbilityInputBindingByAbilityId { Slot = AbilityInputSlot.Slot1, AbilityId = 42 });
            // 능력 부여
            _em.GetBuffer<GrantAbilityRequest>(monster).Add(new GrantAbilityRequest { AbilityId = 42, Level = 1, Source = monster });

            _grant.Update();
            _autoBind.Update();

            Assert.AreEqual(1, _em.GetBuffer<AbilityInputBinding>(monster).Length);

            _bt.Update();
            Assert.AreEqual(1, _em.GetBuffer<PressAbilityInputRequest>(monster).Length);

            _input.Update();
            Assert.AreEqual(1, _em.GetBuffer<TryActivateAbilityRequest>(monster).Length);
        }

        private void CreateTestDatabaseSingleton()
        {
            // AbilityId=42만 있으면 됨(GrantSystem이 DB 조회하므로)
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, 1);
            abilities[0] = new AbilityDefinition
            {
                AbilityId = 42,
                ExecutionType = AbilityExecutionType.ApplyEffectToTarget,
                CooldownDuration = 0f,
                ManaCost = 0f,
                TagRequirements = default,
                ProjectileFlightTime = 0f,
                PrimaryEffectId = 0,
                SecondaryEffectId = 0,
                CleanseTag = GameplayTag.Invalid,
                CooldownEffectId = 0,
                CooldownTag = GameplayTag.Invalid,
            };

            builder.Allocate(ref root.Effects, 0);

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(Allocator.Persistent);
            builder.Dispose();

            var dbEntity = _em.CreateEntity();
            _em.AddComponentData(dbEntity, new AbilityEffectDatabase { Blob = blob });
        }

        private static BlobAssetReference<BehaviorTreeBlob> BuildSinglePressTree()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BehaviorTreeBlob>();
            var nodes = builder.Allocate(ref root.Nodes, 1);
            builder.Allocate(ref root.Children, 0);
            nodes[0] = new BehaviorTreeNode { Type = BtNodeType.Action, Action = BtActionType.PressAbilitySlot, Slot = AbilityInputSlot.Slot1 };
            var blob = builder.CreateBlobAssetReference<BehaviorTreeBlob>(Allocator.Persistent);
            builder.Dispose();
            return blob;
        }
    }
}

