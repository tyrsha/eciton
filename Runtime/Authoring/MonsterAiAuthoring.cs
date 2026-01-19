using Tyrsha.Eciton;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// 몬스터 AI(BehaviorTree)를 베이크하기 위한 Authoring 컴포넌트.
    /// </summary>
    public class MonsterAiAuthoring : MonoBehaviour
    {
        public BehaviorTreeAsset Tree;
        public float DesiredStoppingDistance = 1.5f;
    }

    public class MonsterAiBaker : Baker<MonsterAiAuthoring>
    {
        public override void Bake(MonsterAiAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // BT Blob 생성
            BlobAssetReference<BehaviorTreeBlob> blob = default;
            if (authoring.Tree != null && authoring.Tree.Nodes != null && authoring.Tree.Nodes.Length > 0)
            {
                using var builder = new BlobBuilder(AllocatorManager.Persistent);
                ref var root = ref builder.ConstructRoot<BehaviorTreeBlob>();

                var nodes = builder.Allocate(ref root.Nodes, authoring.Tree.Nodes.Length);

                int totalChildren = 0;
                for (int i = 0; i < authoring.Tree.Nodes.Length; i++)
                    totalChildren += authoring.Tree.Nodes[i]?.Children?.Length ?? 0;

                var children = builder.Allocate(ref root.Children, totalChildren);
                int cursor = 0;

                for (int i = 0; i < authoring.Tree.Nodes.Length; i++)
                {
                    var src = authoring.Tree.Nodes[i];
                    int childCount = src?.Children?.Length ?? 0;
                    int first = cursor;
                    for (int c = 0; c < childCount; c++)
                        children[cursor++] = src.Children[c];

                    nodes[i] = new BehaviorTreeNode
                    {
                        Type = src != null ? src.Type : BtNodeType.Condition,
                        Condition = src != null ? src.Condition : BtConditionType.None,
                        Action = src != null ? src.Action : BtActionType.None,
                        FirstChildIndex = first,
                        ChildCount = childCount,
                        Slot = src != null ? src.Slot : AbilityInputSlot.Slot1,
                        Tag = new GameplayTag { Value = src != null ? src.TagValue : 0 },
                        FloatParam0 = src != null ? src.FloatParam0 : 0f,
                    };
                }

                blob = builder.CreateBlobAssetReference<BehaviorTreeBlob>(AllocatorManager.Persistent);
            }

            AddComponent(entity, new BehaviorTreeAgent { Tree = blob });
            AddComponent(entity, new BehaviorTreeBlackboard { Target = Entity.Null, TargetInRange = 0, DesiredStoppingDistance = authoring.DesiredStoppingDistance });
            AddComponent(entity, new BehaviorTreeLastResult { Status = BtStatus.Failure, LastNodeIndex = -1 });

            // BT 액션이 사용할 수 있도록 버퍼들을 보장(입력/태그)
            AddBuffer<PressAbilityInputRequest>(entity);
            AddBuffer<GameplayTagElement>(entity);
        }
    }
}

