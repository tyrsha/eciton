using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    public enum BtStatus : byte
    {
        Failure = 0,
        Success = 1,
        Running = 2,
    }

    public enum BtNodeType : byte
    {
        Selector = 0,
        Sequence = 1,
        Condition = 2,
        Action = 3,
    }

    public enum BtConditionType : byte
    {
        None = 0,
        HasTarget = 1,
        TargetInRange = 2,
        HasGameplayTag = 3,
        NotHasGameplayTag = 4,
    }

    public enum BtActionType : byte
    {
        None = 0,
        PressAbilitySlot = 1,
        MoveToTarget = 2,
        ClearMoveRequest = 3,
    }

    public struct BehaviorTreeNode
    {
        public BtNodeType Type;
        public BtConditionType Condition;
        public BtActionType Action;

        public int FirstChildIndex; // into Children blob array
        public int ChildCount;

        // Parameters (스텁)
        public AbilityInputSlot Slot;
        public GameplayTag Tag;
        public float FloatParam0;
    }

    public struct BehaviorTreeBlob
    {
        public BlobArray<BehaviorTreeNode> Nodes;
        public BlobArray<int> Children;
    }

    /// <summary>AI 에이전트에 붙는 BT 참조.</summary>
    public struct BehaviorTreeAgent : IComponentData
    {
        public BlobAssetReference<BehaviorTreeBlob> Tree;
    }

    /// <summary>
    /// 몬스터 AI가 참조하는 블랙보드(런타임 가변).
    /// 타겟 선정/사거리 판정은 별도 시스템(게임 프로젝트)에서 채워 넣는 것을 권장.
    /// </summary>
    public struct BehaviorTreeBlackboard : IComponentData
    {
        public Entity Target;
        public byte TargetInRange; // 0/1
        public float DesiredStoppingDistance;
    }

    /// <summary>디버그/테스트용 마지막 평가 결과.</summary>
    public struct BehaviorTreeLastResult : IComponentData
    {
        public BtStatus Status;
        public int LastNodeIndex;
    }

    /// <summary>이동 요청 스텁(실제 이동은 게임 프로젝트가 처리).</summary>
    public struct MoveToTargetRequest : IComponentData
    {
        public Entity Target;
        public float StoppingDistance;
    }
}

