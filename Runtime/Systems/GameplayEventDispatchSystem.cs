using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 각 엔티티의 PendingGameplayEvent를 싱글톤 GameplayEventQueue로 모으는 디스패처.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GameplayEventDispatchSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<GameplayEventQueueSingleton>(out var queueEntity))
                return;

            var em = EntityManager;
            var queue = em.GetBuffer<GameplayEventQueue>(queueEntity);

            // 스텁: 메인 스레드에서 큐로 이동
            Entities.WithoutBurst().ForEach((DynamicBuffer<PendingGameplayEvent> pending) =>
            {
                for (int i = 0; i < pending.Length; i++)
                {
                    queue.Add(new GameplayEventQueue { Event = pending[i].Event });
                }
                pending.Clear();
            }).Run();
        }
    }
}

