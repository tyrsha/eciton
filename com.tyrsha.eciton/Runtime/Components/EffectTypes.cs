using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>활성화된 Effect의 런타임 핸들 스텁.</summary>
    public struct EffectHandle
    {
        public int Value;

        public static EffectHandle Invalid => new EffectHandle { Value = 0 };
        public bool IsValid => Value != 0;
    }

    /// <summary>
    /// GAS의 GameplayEffectSpec에 해당하는 최소 스텁.
    /// </summary>
    public struct EffectSpec
    {
        public int EffectId;
        public int Level;
        public Entity Source;
        public Entity Target;

        public float Duration;
        public bool IsPermanent;

        /// <summary>단일 modifier 스텁(복수는 이후 확장).</summary>
        public AttributeModifier Modifier;
    }

    /// <summary>
    /// ASC에 적용중인 Effect 목록(버퍼) 스텁.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct ActiveEffect : IBufferElementData
    {
        public EffectHandle Handle;
        public int EffectId;
        public int Level;
        public Entity Source;
        public float RemainingTime;
        public bool IsPermanent;
    }

    /// <summary>
    /// Effect 적용 요청 스텁.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct ApplyEffectRequest : IBufferElementData
    {
        public EffectSpec Spec;
    }

    /// <summary>
    /// Effect 제거 요청 스텁.
    /// </summary>
    [InternalBufferCapacity(2)]
    public struct RemoveEffectRequest : IBufferElementData
    {
        public EffectHandle Handle;
    }
}

