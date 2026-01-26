using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 비트마스크 기반 태그 컨테이너 시스템 (32비트).
    /// 태그 추가/제거 요청을 처리하고, 컨테이너의 비트마스크를 업데이트한다.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public partial struct TagContainerSystem32 : ISystem
    {
        [BurstCompile]
        private partial struct ProcessTagRequestsJob : IJobEntity
        {
            public void Execute(
                ref TagContainer32 container,
                ref DynamicBuffer<ActiveTag32> activeTags,
                ref DynamicBuffer<AddTagRequest32> addRequests,
                ref DynamicBuffer<RemoveTagRequest32> removeRequests)
            {
                // Process add requests
                for (int i = 0; i < addRequests.Length; i++)
                {
                    var request = addRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    // 이미 같은 태그가 있는지 확인
                    bool found = false;
                    for (int t = 0; t < activeTags.Length; t++)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            // 스택 증가
                            var tag = activeTags[t];
                            tag.StackCount++;
                            activeTags[t] = tag;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        // 새 태그 추가
                        activeTags.Add(new ActiveTag32
                        {
                            OwnMask = request.OwnMask,
                            ClosureMask = request.ClosureMask,
                            StackCount = 1
                        });
                    }
                }

                // Process remove requests
                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var request = removeRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    for (int t = activeTags.Length - 1; t >= 0; t--)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount--;

                            if (tag.StackCount <= 0)
                            {
                                // 태그 완전 제거
                                activeTags.RemoveAt(t);
                            }
                            else
                            {
                                activeTags[t] = tag;
                            }
                            break;
                        }
                    }
                }

                // Rebuild combined masks
                container.OwnTagsMask = TagBitmask32.Empty;
                container.CombinedMask = TagBitmask32.Empty;

                for (int i = 0; i < activeTags.Length; i++)
                {
                    container.OwnTagsMask = container.OwnTagsMask | activeTags[i].OwnMask;
                    container.CombinedMask = container.CombinedMask | activeTags[i].ClosureMask;
                }

                // Clear requests
                addRequests.Clear();
                removeRequests.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProcessTagRequestsJob().Schedule(state.Dependency);
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public partial struct TagContainerSystem64 : ISystem
    {
        [BurstCompile]
        private partial struct ProcessTagRequestsJob : IJobEntity
        {
            public void Execute(
                ref TagContainer64 container,
                ref DynamicBuffer<ActiveTag64> activeTags,
                ref DynamicBuffer<AddTagRequest64> addRequests,
                ref DynamicBuffer<RemoveTagRequest64> removeRequests)
            {
                for (int i = 0; i < addRequests.Length; i++)
                {
                    var request = addRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    bool found = false;
                    for (int t = 0; t < activeTags.Length; t++)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount++;
                            activeTags[t] = tag;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        activeTags.Add(new ActiveTag64
                        {
                            OwnMask = request.OwnMask,
                            ClosureMask = request.ClosureMask,
                            StackCount = 1
                        });
                    }
                }

                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var request = removeRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    for (int t = activeTags.Length - 1; t >= 0; t--)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount--;

                            if (tag.StackCount <= 0)
                                activeTags.RemoveAt(t);
                            else
                                activeTags[t] = tag;
                            break;
                        }
                    }
                }

                container.OwnTagsMask = TagBitmask64.Empty;
                container.CombinedMask = TagBitmask64.Empty;

                for (int i = 0; i < activeTags.Length; i++)
                {
                    container.OwnTagsMask = container.OwnTagsMask | activeTags[i].OwnMask;
                    container.CombinedMask = container.CombinedMask | activeTags[i].ClosureMask;
                }

                addRequests.Clear();
                removeRequests.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProcessTagRequestsJob().Schedule(state.Dependency);
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public partial struct TagContainerSystem128 : ISystem
    {
        [BurstCompile]
        private partial struct ProcessTagRequestsJob : IJobEntity
        {
            public void Execute(
                ref TagContainer128 container,
                ref DynamicBuffer<ActiveTag128> activeTags,
                ref DynamicBuffer<AddTagRequest128> addRequests,
                ref DynamicBuffer<RemoveTagRequest128> removeRequests)
            {
                for (int i = 0; i < addRequests.Length; i++)
                {
                    var request = addRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    bool found = false;
                    for (int t = 0; t < activeTags.Length; t++)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount++;
                            activeTags[t] = tag;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        activeTags.Add(new ActiveTag128
                        {
                            OwnMask = request.OwnMask,
                            ClosureMask = request.ClosureMask,
                            StackCount = 1
                        });
                    }
                }

                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var request = removeRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    for (int t = activeTags.Length - 1; t >= 0; t--)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount--;

                            if (tag.StackCount <= 0)
                                activeTags.RemoveAt(t);
                            else
                                activeTags[t] = tag;
                            break;
                        }
                    }
                }

                container.OwnTagsMask = TagBitmask128.Empty;
                container.CombinedMask = TagBitmask128.Empty;

                for (int i = 0; i < activeTags.Length; i++)
                {
                    container.OwnTagsMask = container.OwnTagsMask | activeTags[i].OwnMask;
                    container.CombinedMask = container.CombinedMask | activeTags[i].ClosureMask;
                }

                addRequests.Clear();
                removeRequests.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProcessTagRequestsJob().Schedule(state.Dependency);
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EffectRequestSystem))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public partial struct TagContainerSystem256 : ISystem
    {
        [BurstCompile]
        private partial struct ProcessTagRequestsJob : IJobEntity
        {
            public void Execute(
                ref TagContainer256 container,
                ref DynamicBuffer<ActiveTag256> activeTags,
                ref DynamicBuffer<AddTagRequest256> addRequests,
                ref DynamicBuffer<RemoveTagRequest256> removeRequests)
            {
                for (int i = 0; i < addRequests.Length; i++)
                {
                    var request = addRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    bool found = false;
                    for (int t = 0; t < activeTags.Length; t++)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount++;
                            activeTags[t] = tag;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        activeTags.Add(new ActiveTag256
                        {
                            OwnMask = request.OwnMask,
                            ClosureMask = request.ClosureMask,
                            StackCount = 1
                        });
                    }
                }

                for (int i = 0; i < removeRequests.Length; i++)
                {
                    var request = removeRequests[i];
                    if (request.OwnMask.IsEmpty()) continue;

                    for (int t = activeTags.Length - 1; t >= 0; t--)
                    {
                        if (activeTags[t].OwnMask.Equals(request.OwnMask))
                        {
                            var tag = activeTags[t];
                            tag.StackCount--;

                            if (tag.StackCount <= 0)
                                activeTags.RemoveAt(t);
                            else
                                activeTags[t] = tag;
                            break;
                        }
                    }
                }

                container.OwnTagsMask = TagBitmask256.Empty;
                container.CombinedMask = TagBitmask256.Empty;

                for (int i = 0; i < activeTags.Length; i++)
                {
                    container.OwnTagsMask = container.OwnTagsMask | activeTags[i].OwnMask;
                    container.CombinedMask = container.CombinedMask | activeTags[i].ClosureMask;
                }

                addRequests.Clear();
                removeRequests.Clear();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProcessTagRequestsJob().Schedule(state.Dependency);
        }
    }
}
