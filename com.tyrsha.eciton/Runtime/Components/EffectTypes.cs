using Unity.Entities;

namespace Tyrsha.Eciton
{
    public enum EffectStackingPolicy : byte
    {
        None = 0,
        RefreshDuration = 1,
        StackAdditive = 2,
    }

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

        /// <summary>주기(DoT/HoT 등) 여부.</summary>
        public bool IsPeriodic;

        /// <summary>주기 효과의 틱 간격(초). IsPeriodic=true일 때 사용.</summary>
        public float Period;

        /// <summary>
        /// Effect가 활성화된 동안(또는 즉시 효과라면 적용 시점에) 부여할 태그 스텁.
        /// 유효한 태그면 추가/제거 요청이 발행된다.
        /// </summary>
        public GameplayTag GrantedTag;

        /// <summary>
        /// 지속형(비주기) 버프/디버프에서 만료 시 modifier를 되돌릴지 여부.
        /// Add는 -Magnitude, Multiply는 1/Magnitude로 역연산한다(Override는 스텁에서는 무시).
        /// </summary>
        public bool RevertModifierOnEnd;

        /// <summary>스태킹 정책(스텁).</summary>
        public EffectStackingPolicy StackingPolicy;

        /// <summary>최대 스택(0이면 1로 취급).</summary>
        public int MaxStacks;

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

        /// <summary>스텁: 단일 modifier.</summary>
        public AttributeModifier Modifier;

        /// <summary>주기 효과 여부.</summary>
        public bool IsPeriodic;

        /// <summary>틱 간격(초). IsPeriodic=true일 때 사용.</summary>
        public float Period;

        /// <summary>다음 틱까지 남은 시간(초).</summary>
        public float TimeToNextTick;

        /// <summary>활성화된 동안 부여 중인 태그(유효할 때만 의미).</summary>
        public GameplayTag GrantedTag;

        /// <summary>만료 시 modifier 되돌리기 여부.</summary>
        public bool RevertModifierOnEnd;

        public EffectStackingPolicy StackingPolicy;
        public int MaxStacks;
        public int StackCount;
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

    /// <summary>
    /// 특정 태그를 부여한 ActiveEffect들을 제거하는 요청(클렌즈 등).
    /// </summary>
    [InternalBufferCapacity(2)]
    public struct RemoveEffectsWithTagRequest : IBufferElementData
    {
        public GameplayTag Tag;
    }
}

