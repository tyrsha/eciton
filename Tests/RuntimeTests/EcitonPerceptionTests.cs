using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonPerceptionTests
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _perception;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonPerceptionTests");
            _em = _world.EntityManager;
            _perception = _world.CreateSystem<PerceptionSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        [Test]
        public void Perception_sets_nearest_enemy_target_and_inrange_flag()
        {
            var agent = _em.CreateEntity();
            _em.AddComponentData(agent, new Faction { Value = 1 });
            _em.AddComponentData(agent, new PerceptionSensor { Radius = 10f });
            _em.AddComponentData(agent, new AttackRange { Value = 2f });
            _em.AddComponentData(agent, new BehaviorTreeBlackboard { Target = Entity.Null, TargetInRange = 0, DesiredStoppingDistance = 0f });
            _em.AddComponentData(agent, LocalTransform.FromPosition(new float3(0, 0, 0)));

            var enemyFar = _em.CreateEntity();
            _em.AddComponentData(enemyFar, new Faction { Value = 2 });
            _em.AddComponentData(enemyFar, new Targetable());
            _em.AddComponentData(enemyFar, LocalTransform.FromPosition(new float3(5, 0, 0)));

            var enemyNear = _em.CreateEntity();
            _em.AddComponentData(enemyNear, new Faction { Value = 2 });
            _em.AddComponentData(enemyNear, new Targetable());
            _em.AddComponentData(enemyNear, LocalTransform.FromPosition(new float3(3, 0, 0)));

            _perception.Update(_world.Unmanaged);

            var bb = _em.GetComponentData<BehaviorTreeBlackboard>(agent);
            Assert.AreEqual(enemyNear, bb.Target);
            Assert.AreEqual(0, bb.TargetInRange);

            // 적을 사거리 안으로 이동
            _em.SetComponentData(enemyNear, LocalTransform.FromPosition(new float3(1, 0, 0)));
            _perception.Update(_world.Unmanaged);

            bb = _em.GetComponentData<BehaviorTreeBlackboard>(agent);
            Assert.AreEqual(enemyNear, bb.Target);
            Assert.AreEqual(1, bb.TargetInRange);
        }
    }
}

