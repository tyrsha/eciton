using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 태그 요구조건 스텁(AND/NOT만 지원).
    /// </summary>
    public struct TagQuery
    {
        public GameplayTag Required; // 있어야 함(단일)
        public GameplayTag Blocked;  // 있으면 안 됨(단일)
    }

    /// <summary>Ability/Effect의 태그 요구조건을 붙이는 컴포넌트 스텁.</summary>
    public struct TagQueryComponent : IComponentData
    {
        public TagQuery Query;
    }
}

