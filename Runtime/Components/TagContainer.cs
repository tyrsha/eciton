using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 엔티티의 태그 컨테이너 컴포넌트 (32비트).
    /// 엔티티가 보유한 모든 태그의 ClosureMask를 OR한 결과를 저장.
    ///
    /// 사용법:
    /// - HasTag: containerMask.ContainsAll(tagClosureMask)
    /// - HasAnyTag: containerMask.ContainsAny(queryMask)
    /// - HasAllTags: containerMask.ContainsAll(queryMask)
    /// </summary>
    public struct TagContainer32 : IComponentData
    {
        /// <summary>엔티티가 보유한 모든 태그의 ClosureMask OR</summary>
        public TagBitmask32 CombinedMask;

        /// <summary>엔티티가 실제로 가진 태그들의 OwnMask OR (자식 제외)</summary>
        public TagBitmask32 OwnTagsMask;

        public static TagContainer32 Empty => new TagContainer32();

        /// <summary>특정 태그(또는 그 자식 태그)를 가지고 있는지 검사</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasTag(TagBitmask32 tagClosureMask)
        {
            return CombinedMask.ContainsAny(tagClosureMask);
        }

        /// <summary>정확히 해당 태그를 가지고 있는지 검사 (자식 제외)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasExactTag(TagBitmask32 tagOwnMask)
        {
            return OwnTagsMask.ContainsAll(tagOwnMask);
        }

        /// <summary>쿼리의 모든 조건을 만족하는지 검사</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MatchesQuery(TagQueryBitmask32 query)
        {
            return query.Matches(CombinedMask);
        }

        /// <summary>태그가 하나도 없는지 검사</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return OwnTagsMask.IsEmpty();
        }

        /// <summary>가진 태그 개수 (대략적)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int TagCount()
        {
            return OwnTagsMask.PopCount();
        }
    }

    public struct TagContainer64 : IComponentData
    {
        public TagBitmask64 CombinedMask;
        public TagBitmask64 OwnTagsMask;

        public static TagContainer64 Empty => new TagContainer64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasTag(TagBitmask64 tagClosureMask)
        {
            return CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasExactTag(TagBitmask64 tagOwnMask)
        {
            return OwnTagsMask.ContainsAll(tagOwnMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MatchesQuery(TagQueryBitmask64 query)
        {
            return query.Matches(CombinedMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return OwnTagsMask.IsEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int TagCount()
        {
            return OwnTagsMask.PopCount();
        }
    }

    public struct TagContainer128 : IComponentData
    {
        public TagBitmask128 CombinedMask;
        public TagBitmask128 OwnTagsMask;

        public static TagContainer128 Empty => new TagContainer128();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasTag(TagBitmask128 tagClosureMask)
        {
            return CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasExactTag(TagBitmask128 tagOwnMask)
        {
            return OwnTagsMask.ContainsAll(tagOwnMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MatchesQuery(TagQueryBitmask128 query)
        {
            return query.Matches(CombinedMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return OwnTagsMask.IsEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int TagCount()
        {
            return OwnTagsMask.PopCount();
        }
    }

    public struct TagContainer256 : IComponentData
    {
        public TagBitmask256 CombinedMask;
        public TagBitmask256 OwnTagsMask;

        public static TagContainer256 Empty => new TagContainer256();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasTag(TagBitmask256 tagClosureMask)
        {
            return CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasExactTag(TagBitmask256 tagOwnMask)
        {
            return OwnTagsMask.ContainsAll(tagOwnMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool MatchesQuery(TagQueryBitmask256 query)
        {
            return query.Matches(CombinedMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return OwnTagsMask.IsEmpty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int TagCount()
        {
            return OwnTagsMask.PopCount();
        }
    }

    #region Tag Request Buffers

    /// <summary>
    /// 태그 추가 요청 (비트마스크 기반, 32비트).
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct AddTagRequest32 : IBufferElementData
    {
        /// <summary>추가할 태그의 Own 비트마스크</summary>
        public TagBitmask32 OwnMask;

        /// <summary>추가할 태그의 Closure 비트마스크</summary>
        public TagBitmask32 ClosureMask;
    }

    /// <summary>
    /// 태그 제거 요청 (비트마스크 기반, 32비트).
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct RemoveTagRequest32 : IBufferElementData
    {
        /// <summary>제거할 태그의 Own 비트마스크</summary>
        public TagBitmask32 OwnMask;

        /// <summary>제거할 태그의 Closure 비트마스크</summary>
        public TagBitmask32 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct AddTagRequest64 : IBufferElementData
    {
        public TagBitmask64 OwnMask;
        public TagBitmask64 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct RemoveTagRequest64 : IBufferElementData
    {
        public TagBitmask64 OwnMask;
        public TagBitmask64 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct AddTagRequest128 : IBufferElementData
    {
        public TagBitmask128 OwnMask;
        public TagBitmask128 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct RemoveTagRequest128 : IBufferElementData
    {
        public TagBitmask128 OwnMask;
        public TagBitmask128 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct AddTagRequest256 : IBufferElementData
    {
        public TagBitmask256 OwnMask;
        public TagBitmask256 ClosureMask;
    }

    [InternalBufferCapacity(4)]
    public struct RemoveTagRequest256 : IBufferElementData
    {
        public TagBitmask256 OwnMask;
        public TagBitmask256 ClosureMask;
    }

    #endregion

    #region Active Tag Tracking

    /// <summary>
    /// 개별 태그 추적을 위한 버퍼 요소 (32비트).
    /// 태그 제거 시 CombinedMask 재계산에 사용.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct ActiveTag32 : IBufferElementData
    {
        public TagBitmask32 OwnMask;
        public TagBitmask32 ClosureMask;

        /// <summary>스택 카운트 (같은 태그가 여러번 추가된 경우)</summary>
        public int StackCount;
    }

    [InternalBufferCapacity(16)]
    public struct ActiveTag64 : IBufferElementData
    {
        public TagBitmask64 OwnMask;
        public TagBitmask64 ClosureMask;
        public int StackCount;
    }

    [InternalBufferCapacity(16)]
    public struct ActiveTag128 : IBufferElementData
    {
        public TagBitmask128 OwnMask;
        public TagBitmask128 ClosureMask;
        public int StackCount;
    }

    [InternalBufferCapacity(16)]
    public struct ActiveTag256 : IBufferElementData
    {
        public TagBitmask256 OwnMask;
        public TagBitmask256 ClosureMask;
        public int StackCount;
    }

    #endregion
}
