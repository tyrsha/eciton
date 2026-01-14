using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tyrsha.Eciton.Tests
{
    public class EcitonAggroPerceptionTests
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _perception;

        [SetUp]
        public void SetUp()
        {
            _world = new World("EcitonAggroPerceptionTests");
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
        public void Perception_prefers_higher_threat_over_closer_target()
        {
            var agent = _em.CreateEntity();
            _em.AddComponentData(agent, new Faction { Value = 1 });
            _em.AddComponentData(agent, new PerceptionSensor
            {
                Radius = 20f,
                MemorySeconds = 0f,
                ThreatDecayPerSecond = 0f,
                DistanceWeight = 0.001f,
                SwitchHysteresis = 0f,
                RequireLineOfSight = 0
            });
            _em.AddComponentData(agent, new AttackRange { Value = 2f });
            _em.AddComponentData(agent, new BehaviorTreeBlackboard { Target = Entity.Null, TargetInRange = 0, DesiredStoppingDistance = 0f });
            _em.AddComponentData(agent, LocalTransform.FromPosition(new float3(0, 0, 0)));
            _em.AddBuffer<ThreatEntry>(agent);

            var closeLowThreat = CreateTarget(faction: 2, pos: new float3(2, 0, 0), priority: 0f);
            var farHighThreat = CreateTarget(faction: 2, pos: new float3(10, 0, 0), priority: 0f);

            // Threat를 멀리 있는 타겟에 부여
            var threat = _em.GetBuffer<ThreatEntry>(agent);
            threat.Add(new ThreatEntry { Target = farHighThreat, Threat = 100f, LastSeenTime = 0, LastAggroTime = 0 });
            threat.Add(new ThreatEntry { Target = closeLowThreat, Threat = 0f, LastSeenTime = 0, LastAggroTime = 0 });

            _perception.Update(_world.Unmanaged);
            var bb = _em.GetComponentData<BehaviorTreeBlackboard>(agent);
            Assert.AreEqual(farHighThreat, bb.Target);
        }

        [Test]
        public void Perception_prefers_higher_priority_when_threat_equal()
        {
            var agent = _em.CreateEntity();
            _em.AddComponentData(agent, new Faction { Value = 1 });
            _em.AddComponentData(agent, new PerceptionSensor
            {
                Radius = 20f,
                MemorySeconds = 0f,
                ThreatDecayPerSecond = 0f,
                DistanceWeight = 0f,
                SwitchHysteresis = 0f,
                RequireLineOfSight = 0
            });
            _em.AddComponentData(agent, new AttackRange { Value = 2f });
            _em.AddComponentData(agent, new BehaviorTreeBlackboard { Target = Entity.Null, TargetInRange = 0, DesiredStoppingDistance = 0f });
            _em.AddComponentData(agent, LocalTransform.FromPosition(new float3(0, 0, 0)));
            _em.AddBuffer<ThreatEntry>(agent);

            var low = CreateTarget(faction: 2, pos: new float3(5, 0, 0), priority: 0f);
            var high = CreateTarget(faction: 2, pos: new float3(6, 0, 0), priority: 10f);

            _perception.Update(_world.Unmanaged);
            var bb = _em.GetComponentData<BehaviorTreeBlackboard>(agent);
            Assert.AreEqual(high, bb.Target);
        }

        [Test]
        public void Perception_los_buffer_filters_candidates_when_required()
        {
            var agent = _em.CreateEntity();
            _em.AddComponentData(agent, new Faction { Value = 1 });
            _em.AddComponentData(agent, new PerceptionSensor
            {
                Radius = 20f,
                MemorySeconds = 0f,
                ThreatDecayPerSecond = 0f,
                DistanceWeight = 0f,
                SwitchHysteresis = 0f,
                RequireLineOfSight = 1
            });
            _em.AddComponentData(agent, new AttackRange { Value = 2f });
            _em.AddComponentData(agent, new BehaviorTreeBlackboard { Target = Entity.Null, TargetInRange = 0, DesiredStoppingDistance = 0f });
            _em.AddComponentData(agent, LocalTransform.FromPosition(new float3(0, 0, 0)));
            _em.AddBuffer<ThreatEntry>(agent);
            _em.AddBuffer<VisibleTarget>(agent);

            var a = CreateTarget(faction: 2, pos: new float3(2, 0, 0), priority: 0f);
            var b = CreateTarget(faction: 2, pos: new float3(3, 0, 0), priority: 0f);

            // b만 보이는 것으로 설정
            var visible = _em.GetBuffer<VisibleTarget>(agent);
            visible.Add(new VisibleTarget { Target = b });

            _perception.Update(_world.Unmanaged);
            var bb = _em.GetComponentData<BehaviorTreeBlackboard>(agent);
            Assert.AreEqual(b, bb.Target);
        }

        private Entity CreateTarget(int faction, float3 pos, float priority)
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, new Faction { Value = faction });
            _em.AddComponentData(e, new Targetable());
            _em.AddComponentData(e, LocalTransform.FromPosition(pos));
            _em.AddComponentData(e, new TargetPriority { Weight = priority });
            return e;
        }
    }
}

