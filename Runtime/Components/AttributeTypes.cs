using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Attribute 식별자 스텁. 프로젝트마다 enum/해시/ID 테이블로 확장 가능.
    /// </summary>
    public enum AttributeId : byte
    {
        Health = 1,
        Mana = 2,
        Strength = 3,
        Agility = 4,
        Shield = 5,
        MoveSpeed = 6,
    }

    public enum AttributeModOp : byte
    {
        Add = 0,
        Multiply = 1,
        Override = 2,
    }

    /// <summary>
    /// 속성 변경(Modifier) 스텁.
    /// </summary>
    public struct AttributeModifier
    {
        public AttributeId Attribute;
        public AttributeModOp Op;
        public float Magnitude;

        /// <summary>
        /// 데미지 타입 스텁. Health에 음수 Add가 들어올 때만 의미가 있다.
        /// </summary>
        public DamageType DamageType;
    }

    /// <summary>
    /// Attribute 변경 요청 스텁(Effect 처리 결과를 AttributeSystem이 반영할 수 있게).
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct ApplyAttributeModifierRequest : IBufferElementData
    {
        public AttributeModifier Modifier;
    }
}

