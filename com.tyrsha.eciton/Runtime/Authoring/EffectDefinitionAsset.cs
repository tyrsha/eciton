using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>Effect 정의(Authoring) 스텁.</summary>
    public class EffectDefinitionAsset : ScriptableObject
    {
        public int EffectId;
        public float Duration;
        public bool IsPeriodic;
        public float Period;
    }
}

