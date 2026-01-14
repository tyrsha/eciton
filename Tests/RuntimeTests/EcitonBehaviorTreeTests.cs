using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonBehaviorTreeTests
    {
        private World _world;
        private EntityManager _em;

        private SystemHandle _bt;
        private SystemHandle _input;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonBehaviorTreeTests");
            _em = _world.EntityManager;

            _bt = _world.CreateSystem<BehaviorTreeTickSystem>();
            _input = _world.CreateSystem<AbilityInputSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void BehaviorTree_press_ability_slot_emits_input_request()
        {
            // BT: Sequence(HasTarget, TargetInRange, PressSlot1)
            var builder = new BlobBuilder(AllocatorManager.Temp);
            ref var root = ref builder.ConstructRoot<BehaviorTreeBlob>();

            var nodes = builder.Allocate(ref root.Nodes, 4);
            var children = builder.Allocate(ref root.Children, 3);
            children[0] = 1;
            children[1] = 2;
            children[2] = 3;

            nodes[0] = new BehaviorTreeNode { Type = BtNodeType.Sequence, FirstChildIndex = 0, ChildCount = 3 };
            nodes[1] = new BehaviorTreeNode { Type = BtNodeType.Condition, Condition = BtConditionType.HasTarget };
            nodes[2] = new BehaviorTreeNode { Type = BtNodeType.Condition, Condition = BtConditionType.TargetInRange };
            nodes[3] = new BehaviorTreeNode { Type = BtNodeType.Action, Action = BtActionType.PressAbilitySlot, Slot = AbilityInputSlot.Slot1 };

            var blob = builder.CreateBlobAssetReference<BehaviorTreeBlob>(AllocatorManager.Persistent);
            builder.Dispose();

            var agent = _em.CreateEntity();
            _em.AddComponentData(agent, new BehaviorTreeAgent { Tree = blob });
            _em.AddComponentData(agent, new BehaviorTreeBlackboard { Target = new Entity { Index = 123, Version = 1 }, TargetInRange = 1, DesiredStoppingDistance = 1.5f });
            _em.AddComponentData(agent, new BehaviorTreeLastResult { Status = BtStatus.Failure, LastNodeIndex = -1 });
            _em.AddBuffer<PressAbilityInputRequest>(agent);
            _em.AddBuffer<AbilityInputBinding>(agent);
            _em.AddBuffer<TryActivateAbilityRequest>(agent);

            _bt.Update(_world.Unmanaged);

            Assert.AreEqual(1, _em.GetBuffer<PressAbilityInputRequest>(agent).Length);

            // InputSystem 변환까지(바인딩이 없으므로 TryActivate는 생성되지 않는다)
            _input.Update(_world.Unmanaged);
            Assert.AreEqual(0, _em.GetBuffer<TryActivateAbilityRequest>(agent).Length);
        }
    }
}

