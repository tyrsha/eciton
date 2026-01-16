using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>프로젝트에서 확장 가능한 입력 슬롯 식별자(예: QWER/1~5 등).</summary>
    public enum AbilityInputSlot : byte
    {
        Slot1 = 1,
        Slot2 = 2,
        Slot3 = 3,
        Slot4 = 4,
        Slot5 = 5,
    }

    /// <summary>슬롯 -> AbilityHandle 바인딩.</summary>
    [InternalBufferCapacity(8)]
    public struct AbilityInputBinding : IBufferElementData
    {
        public AbilityInputSlot Slot;
        public AbilityHandle Handle;
    }

    /// <summary>
    /// (데이터 드리븐) 슬롯 -> AbilityId 바인딩.
    /// AbilityGrantSystem으로 능력이 부여된 뒤, AutoBind 시스템이 Handle 바인딩을 완성한다.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct AbilityInputBindingByAbilityId : IBufferElementData
    {
        public AbilityInputSlot Slot;
        public int AbilityId;
    }

    /// <summary>
    /// 프레젠테이션/입력 레이어가 누르는 입력 이벤트를 코어로 전달하는 요청.
    /// 타겟 지정은 TargetData로 확장 가능.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct PressAbilityInputRequest : IBufferElementData
    {
        public AbilityInputSlot Slot;
        public TargetData TargetData;
    }
}

