using Unity.Entities;

namespace Tyrsha.Eciton
{
    public struct EffectBase : IComponentData
    {
        public float Duration;     // 효과 지속 시간
        public float Value;        // 효과 강도
        public bool IsPermanent;   // 영구적인 효과인지 여부
    }
}