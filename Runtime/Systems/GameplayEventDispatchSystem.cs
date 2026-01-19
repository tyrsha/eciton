using Unity.Entities;
using Unity.Collections;

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
            using var query = GetEntityQuery(ComponentType.ReadWrite<PendingGameplayEvent>());
            using var entities = query.ToEntityArray(Allocator.Temp);

            for (int e = 0; e < entities.Length; e++)
            {
                var pending = em.GetBuffer<PendingGameplayEvent>(entities[e]);
                for (int i = 0; i < pending.Length; i++)
                {
                    queue.Add(new GameplayEventQueue { Event = pending[i].Event });
                }
                pending.Clear();
            }
        }
    }
}

