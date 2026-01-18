using Tyrsha.Eciton;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>Effect 정의(Authoring) 스텁.</summary>
    public class EffectDefinitionAsset : ScriptableObject
    {
        public int EffectId;
        public float Duration;
        public bool IsPermanent;
        public bool IsPeriodic;
        public float Period;

        public int GrantedTagValue;
        public int BlockedByTagValue;
        public bool RevertModifierOnEnd;
        public EffectStackingPolicy StackingPolicy;
        public int MaxStacks = 1;

        // 다중 modifier 지원(우선 사용)
        public AttributeModifierEntry[] Modifiers;

        // 호환용 단일 modifier(Modifiers가 비어있으면 사용)
        public AttributeId ModifierAttribute;
        public AttributeModOp ModifierOp;
        public float ModifierMagnitude;
        public DamageType ModifierDamageType;
    }
}

