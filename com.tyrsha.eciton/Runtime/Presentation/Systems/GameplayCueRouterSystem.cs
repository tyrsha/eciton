using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어의 Add/RemoveGameplayTagRequest를 관찰해 GameplayCueEvent로 변환하는 스텁.
    /// (코어 로직을 건드리지 않고, 프레젠테이션에서만 연출 이벤트를 생성)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GameplayTagSystem))]
    public partial class GameplayCueRouterSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            Entities.WithoutBurst().ForEach((
                Entity e,
                in AbilitySystemComponent asc,
                DynamicBuffer<AddGameplayTagRequest> addReq,
                DynamicBuffer<RemoveGameplayTagRequest> removeReq) =>
            {
                _ = asc;

                if (!em.HasBuffer<GameplayCueEvent>(e))
                    em.AddBuffer<GameplayCueEvent>(e);
                var cues = em.GetBuffer<GameplayCueEvent>(e);

                for (int i = 0; i < addReq.Length; i++)
                {
                    var tag = addReq[i].Tag;
                    if (tag.IsValid)
                        cues.Add(new GameplayCueEvent { Type = GameplayCueEventType.TagAdded, Tag = tag });
                }

                for (int i = 0; i < removeReq.Length; i++)
                {
                    var tag = removeReq[i].Tag;
                    if (tag.IsValid)
                        cues.Add(new GameplayCueEvent { Type = GameplayCueEventType.TagRemoved, Tag = tag });
                }
            }).Run();
        }
    }
}

