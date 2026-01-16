using System;
using Tyrsha.Eciton;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    [Serializable]
    public class BehaviorTreeNodeAsset
    {
        public BtNodeType Type;

        public BtConditionType Condition;
        public BtActionType Action;

        public int[] Children;

        public AbilityInputSlot Slot;
        public int TagValue;
        public float FloatParam0;
    }

    [CreateAssetMenu(menuName = "Eciton/AI/Behavior Tree", fileName = "EcitonBehaviorTree")]
    public class BehaviorTreeAsset : ScriptableObject
    {
        /// <summary>루트 노드는 0번 인덱스로 가정.</summary>
        public BehaviorTreeNodeAsset[] Nodes;
    }
}

