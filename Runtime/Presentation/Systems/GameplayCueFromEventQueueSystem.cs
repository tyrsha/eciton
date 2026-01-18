using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어의 GameplayEventQueue를 소비해 GameplayCueEvent로 변환하는 시스템.
    /// (예: EffectApplied/Removed -> 해당 Effect의 GrantedTag를 TagAdded/Removed cue로 변환)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventDispatchSystem))]
    public class GameplayCueFromEventQueueSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<GameplayEventQueueSingleton>(out var queueEntity))
                return;

            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = EntityManager;
            var queue = em.GetBuffer<GameplayEventQueue>(queueEntity);

            // 스텁: 메인 스레드에서 처리 후 큐 비움
            for (int i = 0; i < queue.Length; i++)
            {
                var evt = queue[i].Event;
                if (evt.Target == Entity.Null || !em.Exists(evt.Target))
                    continue;

                if (!em.HasBuffer<GameplayCueEvent>(evt.Target))
                    em.AddBuffer<GameplayCueEvent>(evt.Target);
                var cues = em.GetBuffer<GameplayCueEvent>(evt.Target);

                if (evt.Type == GameplayEventType.EffectApplied || evt.Type == GameplayEventType.EffectRemoved)
                {
                    if (AbilityEffectDatabaseLookup.TryGetEffect(db, evt.Id, out var def))
                    {
                        if (def.GrantedTag.IsValid)
                        {
                            cues.Add(new GameplayCueEvent
                            {
                                Type = evt.Type == GameplayEventType.EffectApplied ? GameplayCueEventType.TagAdded : GameplayCueEventType.TagRemoved,
                                Tag = def.GrantedTag
                            });
                        }
                    }
                }
            }

            queue.Clear();
        }
    }
}

