using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GAS의 GameplayTag에 해당하는 최소 표현 스텁.
    /// 구현 상세(문자열/해시/트리)는 이후 확장한다.
    /// </summary>
    public struct GameplayTag
    {
        public int Value;

        public static GameplayTag Invalid => new GameplayTag { Value = 0 };
        public bool IsValid => Value != 0;
    }

    /// <summary>
    /// ASC 엔티티에 붙는 태그 컨테이너(버퍼) 스텁.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct GameplayTagElement : IBufferElementData
    {
        public GameplayTag Tag;
    }

    /// <summary>
    /// 태그 추가 요청 스텁.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct AddGameplayTagRequest : IBufferElementData
    {
        public GameplayTag Tag;
    }

    /// <summary>
    /// 태그 제거 요청 스텁.
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct RemoveGameplayTagRequest : IBufferElementData
    {
        public GameplayTag Tag;
    }
}

