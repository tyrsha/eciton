using System;
using Tyrsha.Eciton;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    [Serializable]
    public struct AttributeModifierEntry
    {
        public AttributeId Attribute;
        public AttributeModOp Op;
        public float Magnitude;
        public DamageType DamageType;
    }
}

