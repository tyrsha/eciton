using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Attribute 집계기를 위한 Modifier(버퍼) 스텁.
    /// 코어는 이 버퍼를 읽어 AggregatedAttributeData를 계산할 수 있다.
    /// (현재 프로젝트는 ApplyAttributeModifierRequest 방식도 유지)
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct AttributeModifierElement : IBufferElementData
    {
        public int SourceEffectId;
        public AttributeModifier Modifier;
    }

    /// <summary>
    /// 집계된 결과를 저장하는 컴포넌트 스텁(예: UI/AI가 읽기 좋게).
    /// </summary>
    public struct AggregatedAttributeData : IComponentData
    {
        public float Health;
        public float Mana;
        public float Strength;
        public float Agility;
        public float Shield;
        public float MoveSpeed;
    }
}

