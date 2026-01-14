using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 태그 추가/제거 요청을 처리하는 최소 스텁 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public partial struct GameplayTagSystem : ISystem
    {
        [BurstCompile]
        private partial struct GameplayTagJob : IJobEntity
        {
            public void Execute(
                DynamicBuffer<GameplayTagElement> tags,
                DynamicBuffer<AddGameplayTagRequest> addRequests,
                DynamicBuffer<RemoveGameplayTagRequest> removeRequests)
            {
                // Add
                for (int i = 0; i < addRequests.Length; i++)
                {
                    var tag = addRequests[i].Tag;
                    if (!tag.IsValid) continue;

                    bool exists = false;
                    for (int t = 0; t < tags.Length; t++)
                    {
                        if (tags[t].Tag.Value == tag.Value)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                        tags.Add(new GameplayTagElement { Tag = tag });
                }

                // Remove
                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var tag = removeRequests[i].Tag;
                    if (!tag.IsValid) continue;

                    for (int t = tags.Length - 1; t >= 0; t--)
                    {
                        if (tags[t].Tag.Value == tag.Value)
                            tags.RemoveAt(t);
                    }
                }

                addRequests.Clear();
                removeRequests.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new GameplayTagJob().Schedule(state.Dependency);
        }
    }
}

