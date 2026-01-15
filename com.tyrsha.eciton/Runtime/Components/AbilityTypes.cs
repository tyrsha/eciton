using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>부여된 Ability의 런타임 핸들 스텁.</summary>
    public struct AbilityHandle
    {
        public int Value;
        public int Version;

        public static AbilityHandle Invalid => new AbilityHandle { Value = 0 };
        public bool IsValid => Value != 0;
    }

    /// <summary>
    /// GAS의 GameplayAbilitySpec(부여된 능력 정보)에 해당하는 최소 스텁.
    /// 실제 정의/데이터(스크립터블 등)는 이후 확장한다.
    /// </summary>
    public struct AbilitySpec
    {
        public AbilityHandle Handle;
        public int AbilityId;
        public int Level;
        public Entity Source;

        public float CooldownDuration;
        public float ManaCost;

        public TagQuery TagRequirements;
    }

    /// <summary>
    /// ASC에 부여된 Ability 목록(버퍼) 스텁.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct GrantedAbility : IBufferElementData
    {
        public AbilityHandle Handle;
        public int AbilityId;
        public int Level;
        public Entity Source;

        public float CooldownDuration;
        public float CooldownRemaining;
        public float ManaCost;

        public TagQuery TagRequirements;
    }

    /// <summary>
    /// Ability 활성화 요청 스텁(입력/AI 등에서 쌓고 시스템이 처리).
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct TryActivateAbilityRequest : IBufferElementData
    {
        public AbilityHandle Handle;
        public Entity Target;

        public TargetData TargetData;
    }

    /// <summary>
    /// Ability 부여 요청(데이터 드리븐). DB로부터 GrantedAbility를 생성한다.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct GrantAbilityRequest : IBufferElementData
    {
        public int AbilityId;
        public int Level;
        public Entity Source;
    }

    /// <summary>
    /// Ability 취소 요청 스텁.
    /// </summary>
    [InternalBufferCapacity(2)]
    public struct CancelAbilityRequest : IBufferElementData
    {
        public AbilityHandle Handle;
    }
}

