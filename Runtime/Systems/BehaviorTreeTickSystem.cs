using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// BehaviorTreeBlob을 DOTS에서 평가하는 최소 런타임.
    /// - 상태 저장 없이 매 프레임 루트부터 평가(Stateless BT).
    /// - Action은 ECS 요청/컴포넌트로만 표현(이동/공격 등 실제 구현은 게임 프로젝트에서 처리).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityInputSystem))]
    public class BehaviorTreeTickSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity e, ref BehaviorTreeBlackboard bb, in BehaviorTreeAgent agent, ref BehaviorTreeLastResult last) =>
            {
                if (!agent.Tree.IsCreated || agent.Tree.Value.Nodes.Length == 0)
                {
                    last.Status = BtStatus.Failure;
                    last.LastNodeIndex = -1;
                    return;
                }

                last.LastNodeIndex = 0;
                last.Status = TickNode(em, e, bb, agent, 0);
            }).Run();
        }

        private static BtStatus TickNode(EntityManager em, Entity entity, BehaviorTreeBlackboard bb, BehaviorTreeAgent agent, int nodeIndex)
        {
            ref var tree = ref agent.Tree.Value;
            var node = tree.Nodes[nodeIndex];

            switch (node.Type)
            {
                case BtNodeType.Selector:
                    return TickSelector(em, entity, bb, agent, node);
                case BtNodeType.Sequence:
                    return TickSequence(em, entity, bb, agent, node);
                case BtNodeType.Condition:
                    return TickCondition(em, entity, bb, node);
                case BtNodeType.Action:
                    return TickAction(em, entity, bb, node);
                default:
                    return BtStatus.Failure;
            }
        }

        private static BtStatus TickSelector(EntityManager em, Entity entity, BehaviorTreeBlackboard bb, BehaviorTreeAgent agent, BehaviorTreeNode node)
        {
            ref var tree = ref agent.Tree.Value;
            for (int i = 0; i < node.ChildCount; i++)
            {
                int child = tree.Children[node.FirstChildIndex + i];
                var status = TickNode(em, entity, bb, agent, child);
                if (status == BtStatus.Success)
                    return BtStatus.Success;
                if (status == BtStatus.Running)
                    return BtStatus.Running;
            }
            return BtStatus.Failure;
        }

        private static BtStatus TickSequence(EntityManager em, Entity entity, BehaviorTreeBlackboard bb, BehaviorTreeAgent agent, BehaviorTreeNode node)
        {
            ref var tree = ref agent.Tree.Value;
            for (int i = 0; i < node.ChildCount; i++)
            {
                int child = tree.Children[node.FirstChildIndex + i];
                var status = TickNode(em, entity, bb, agent, child);
                if (status == BtStatus.Failure)
                    return BtStatus.Failure;
                if (status == BtStatus.Running)
                    return BtStatus.Running;
            }
            return BtStatus.Success;
        }

        private static BtStatus TickCondition(EntityManager em, Entity entity, BehaviorTreeBlackboard bb, BehaviorTreeNode node)
        {
            switch (node.Condition)
            {
                case BtConditionType.HasTarget:
                    return bb.Target != Entity.Null ? BtStatus.Success : BtStatus.Failure;
                case BtConditionType.TargetInRange:
                    return bb.TargetInRange != 0 ? BtStatus.Success : BtStatus.Failure;
                case BtConditionType.HasGameplayTag:
                    return HasTag(em, entity, node.Tag.Value) ? BtStatus.Success : BtStatus.Failure;
                case BtConditionType.NotHasGameplayTag:
                    return HasTag(em, entity, node.Tag.Value) ? BtStatus.Failure : BtStatus.Success;
                default:
                    return BtStatus.Failure;
            }
        }

        private static BtStatus TickAction(EntityManager em, Entity entity, BehaviorTreeBlackboard bb, BehaviorTreeNode node)
        {
            switch (node.Action)
            {
                case BtActionType.PressAbilitySlot:
                    {
                        if (!em.HasBuffer<PressAbilityInputRequest>(entity))
                            em.AddBuffer<PressAbilityInputRequest>(entity);
                        var inputs = em.GetBuffer<PressAbilityInputRequest>(entity);
                        inputs.Add(new PressAbilityInputRequest
                        {
                            Slot = node.Slot,
                            TargetData = new TargetData { Target = bb.Target }
                        });
                        return BtStatus.Success;
                    }
                case BtActionType.MoveToTarget:
                    {
                        // 이동 요청은 유지되는 IComponentData로 표현(게임 프로젝트가 소비/삭제)
                        if (bb.Target == Entity.Null)
                            return BtStatus.Failure;
                        em.AddComponentData(entity, new MoveToTargetRequest
                        {
                            Target = bb.Target,
                            StoppingDistance = bb.DesiredStoppingDistance <= 0f ? node.FloatParam0 : bb.DesiredStoppingDistance
                        });
                        return BtStatus.Running;
                    }
                case BtActionType.ClearMoveRequest:
                    {
                        if (em.HasComponent<MoveToTargetRequest>(entity))
                            em.RemoveComponent<MoveToTargetRequest>(entity);
                        return BtStatus.Success;
                    }
                default:
                    return BtStatus.Failure;
            }
        }

        private static bool HasTag(EntityManager em, Entity entity, int tagValue)
        {
            if (tagValue == 0 || !em.HasBuffer<GameplayTagElement>(entity))
                return false;
            var tags = em.GetBuffer<GameplayTagElement>(entity);
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i].Tag.Value == tagValue)
                    return true;
            }
            return false;
        }
    }
}

