using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어 GameplayEventQueue를 HUD용 로그로 복제하는 시스템.
    /// 큐 자체를 소비/클리어하지 않고, 다른 시스템이 소비하기 전에 복제한다.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventDispatchSystem))]
    [UpdateBefore(typeof(GameplayCueFromEventQueueSystem))]
    public partial struct GameplayEventLogSystem : ISystem
    {
        private const int MaxEntries = 256;

        public void OnCreate(ref SystemState state)
        {
            var em = state.EntityManager;
            var q = state.GetEntityQuery(ComponentType.ReadOnly<GameplayEventLogSingleton>());
            if (q.CalculateEntityCount() > 0)
                return;

            var e = em.CreateEntity();
            em.AddComponentData(e, new GameplayEventLogSingleton());
            em.AddBuffer<GameplayEventLogEntry>(e);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<GameplayEventQueueSingleton>(out var queueEntity))
                return;
            if (!SystemAPI.TryGetSingletonEntity<GameplayEventLogSingleton>(out var logEntity))
                return;

            var em = state.EntityManager;
            var queue = em.GetBuffer<GameplayEventQueue>(queueEntity);
            var log = em.GetBuffer<GameplayEventLogEntry>(logEntity);

            double now = SystemAPI.Time.ElapsedTime;
            for (int i = 0; i < queue.Length; i++)
            {
                var evt = queue[i].Event;
                log.Add(new GameplayEventLogEntry
                {
                    Timestamp = now,
                    Type = evt.Type,
                    Source = evt.Source,
                    Target = evt.Target,
                    Id = evt.Id,
                    Magnitude = evt.Magnitude
                });
            }

            // tail 유지
            if (log.Length > MaxEntries)
            {
                int remove = log.Length - MaxEntries;
                log.RemoveRange(0, remove);
            }
        }
    }
}

