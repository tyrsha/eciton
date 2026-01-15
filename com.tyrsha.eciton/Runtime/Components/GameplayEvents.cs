using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>게임플레이 이벤트 타입(프로젝트에서 확장).</summary>
    public enum GameplayEventType : int
    {
        None = 0,
        DamageApplied = 1,
        HealApplied = 2,
        EffectApplied = 3,
        EffectRemoved = 4,
        AbilityActivated = 5,
    }

    /// <summary>
    /// ECS 이벤트 버스에 쌓이는 이벤트 스텁.
    /// </summary>
    public struct GameplayEvent : IBufferElementData
    {
        public GameplayEventType Type;
        public Entity Source;
        public Entity Target;
        public int Id;          // abilityId/effectId 등
        public float Magnitude; // damage/heal 등
    }

    /// <summary>이벤트 큐(싱글톤 엔티티에 붙는 버퍼).</summary>
    [InternalBufferCapacity(32)]
    public struct GameplayEventQueue : IBufferElementData
    {
        public GameplayEvent Event;
    }

    /// <summary>
    /// 이벤트 큐를 보장하는 부트스트랩 시스템.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameplayEventQueueBootstrapSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            var em = EntityManager;
            using var q = em.CreateEntityQuery(typeof(GameplayEventQueueSingleton));
            if (q.CalculateEntityCount() > 0)
                return;

            var e = em.CreateEntity();
            em.AddComponentData(e, new GameplayEventQueueSingleton());
            em.AddBuffer<GameplayEventQueue>(e);
        }

        protected override void OnUpdate() { }
    }

    /// <summary>이벤트 큐 싱글톤 마커.</summary>
    public struct GameplayEventQueueSingleton : IComponentData { }
}

