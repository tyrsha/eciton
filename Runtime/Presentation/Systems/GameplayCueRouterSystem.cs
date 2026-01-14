using Unity.Entities;
using Tyrsha.Eciton;
using Unity.Collections;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어의 Add/RemoveGameplayTagRequest를 관찰해 GameplayCueEvent로 변환하는 스텁.
    /// (코어 로직을 건드리지 않고, 프레젠테이션에서만 연출 이벤트를 생성)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GameplayTagSystem))]
    public partial struct GameplayCueRouterSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            var query = state.GetEntityQuery(
                ComponentType.ReadOnly<AbilitySystemComponent>(),
                ComponentType.ReadOnly<AddGameplayTagRequest>(),
                ComponentType.ReadOnly<RemoveGameplayTagRequest>());
            using var entities = query.ToEntityArray(AllocatorManager.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var e = entities[i];
                _ = em.GetComponentData<AbilitySystemComponent>(e);
                var addReq = em.GetBuffer<AddGameplayTagRequest>(e);
                var removeReq = em.GetBuffer<RemoveGameplayTagRequest>(e);

                if (!em.HasBuffer<GameplayCueEvent>(e))
                    em.AddBuffer<GameplayCueEvent>(e);
                var cues = em.GetBuffer<GameplayCueEvent>(e);

                for (int j = 0; j < addReq.Length; j++)
                {
                    var tag = addReq[j].Tag;
                    if (tag.IsValid)
                        cues.Add(new GameplayCueEvent { Type = GameplayCueEventType.TagAdded, Tag = tag });
                }

                for (int k = 0; k < removeReq.Length; k++)
                {
                    var tag = removeReq[k].Tag;
                    if (tag.IsValid)
                        cues.Add(new GameplayCueEvent { Type = GameplayCueEventType.TagRemoved, Tag = tag });
                }
            }
        }
    }
}

