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
        public bool RevertModifierOnEnd;
        public EffectStackingPolicy StackingPolicy;
        public int MaxStacks = 1;

        public AttributeId ModifierAttribute;
        public AttributeModOp ModifierOp;
        public float ModifierMagnitude;
    }
}

