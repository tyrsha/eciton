using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// TagContainer 확장 메서드.
    /// 태그 추가/제거 요청을 편리하게 생성할 수 있다.
    /// </summary>
    [BurstCompile]
    public static class TagContainerExtensions
    {
        #region 32-bit Extensions

        /// <summary>
        /// 태그 추가 요청을 버퍼에 추가한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest32> buffer,
            TagBitmask32 ownMask,
            TagBitmask32 closureMask)
        {
            buffer.Add(new AddTagRequest32 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        /// <summary>
        /// 태그 정의를 사용하여 추가 요청을 생성한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest32> buffer,
            TagDefinition32 definition)
        {
            buffer.Add(new AddTagRequest32 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        /// <summary>
        /// 태그 제거 요청을 버퍼에 추가한다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest32> buffer,
            TagBitmask32 ownMask,
            TagBitmask32 closureMask)
        {
            buffer.Add(new RemoveTagRequest32 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest32> buffer,
            TagDefinition32 definition)
        {
            buffer.Add(new RemoveTagRequest32 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        /// <summary>
        /// 태그 컨테이너가 특정 태그(또는 그 자식)를 가지고 있는지 확인한다.
        /// Parent Closure 방식으로, 부모 태그를 쿼리하면 모든 자식 태그도 매칭된다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTagOrChild(this TagContainer32 container, TagBitmask32 tagClosureMask)
        {
            return container.CombinedMask.ContainsAny(tagClosureMask);
        }

        /// <summary>
        /// 정확히 해당 태그를 가지고 있는지 확인 (자식 제외).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasExactTag(this TagContainer32 container, TagBitmask32 tagOwnMask)
        {
            return container.OwnTagsMask.ContainsAll(tagOwnMask);
        }

        #endregion

        #region 64-bit Extensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest64> buffer,
            TagBitmask64 ownMask,
            TagBitmask64 closureMask)
        {
            buffer.Add(new AddTagRequest64 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest64> buffer,
            TagDefinition64 definition)
        {
            buffer.Add(new AddTagRequest64 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest64> buffer,
            TagBitmask64 ownMask,
            TagBitmask64 closureMask)
        {
            buffer.Add(new RemoveTagRequest64 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest64> buffer,
            TagDefinition64 definition)
        {
            buffer.Add(new RemoveTagRequest64 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTagOrChild(this TagContainer64 container, TagBitmask64 tagClosureMask)
        {
            return container.CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasExactTag(this TagContainer64 container, TagBitmask64 tagOwnMask)
        {
            return container.OwnTagsMask.ContainsAll(tagOwnMask);
        }

        #endregion

        #region 128-bit Extensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest128> buffer,
            TagBitmask128 ownMask,
            TagBitmask128 closureMask)
        {
            buffer.Add(new AddTagRequest128 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest128> buffer,
            TagDefinition128 definition)
        {
            buffer.Add(new AddTagRequest128 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest128> buffer,
            TagBitmask128 ownMask,
            TagBitmask128 closureMask)
        {
            buffer.Add(new RemoveTagRequest128 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest128> buffer,
            TagDefinition128 definition)
        {
            buffer.Add(new RemoveTagRequest128 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTagOrChild(this TagContainer128 container, TagBitmask128 tagClosureMask)
        {
            return container.CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasExactTag(this TagContainer128 container, TagBitmask128 tagOwnMask)
        {
            return container.OwnTagsMask.ContainsAll(tagOwnMask);
        }

        #endregion

        #region 256-bit Extensions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest256> buffer,
            TagBitmask256 ownMask,
            TagBitmask256 closureMask)
        {
            buffer.Add(new AddTagRequest256 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestAddTag(
            this DynamicBuffer<AddTagRequest256> buffer,
            TagDefinition256 definition)
        {
            buffer.Add(new AddTagRequest256 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest256> buffer,
            TagBitmask256 ownMask,
            TagBitmask256 closureMask)
        {
            buffer.Add(new RemoveTagRequest256 { OwnMask = ownMask, ClosureMask = closureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RequestRemoveTag(
            this DynamicBuffer<RemoveTagRequest256> buffer,
            TagDefinition256 definition)
        {
            buffer.Add(new RemoveTagRequest256 { OwnMask = definition.OwnMask, ClosureMask = definition.ClosureMask });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTagOrChild(this TagContainer256 container, TagBitmask256 tagClosureMask)
        {
            return container.CombinedMask.ContainsAny(tagClosureMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasExactTag(this TagContainer256 container, TagBitmask256 tagOwnMask)
        {
            return container.OwnTagsMask.ContainsAll(tagOwnMask);
        }

        #endregion
    }

    /// <summary>
    /// 태그 쿼리 빌더 (32비트).
    /// 메서드 체이닝으로 쿼리를 구성할 수 있다.
    /// </summary>
    public struct TagQueryBuilder32
    {
        public TagQueryBitmask32 Query;

        public static TagQueryBuilder32 Create() => new TagQueryBuilder32();

        /// <summary>이 태그가 필수 (AND)</summary>
        public TagQueryBuilder32 Require(TagBitmask32 closureMask)
        {
            Query.RequiredMask = Query.RequiredMask | closureMask;
            return this;
        }

        /// <summary>이 태그가 있으면 안됨 (NAND)</summary>
        public TagQueryBuilder32 Block(TagBitmask32 ownMask)
        {
            Query.BlockedMask = Query.BlockedMask | ownMask;
            return this;
        }

        /// <summary>이 태그들 중 하나라도 필요 (OR)</summary>
        public TagQueryBuilder32 RequireAny(TagBitmask32 closureMask)
        {
            Query.AnyRequiredMask = Query.AnyRequiredMask | closureMask;
            return this;
        }

        public TagQueryBitmask32 Build() => Query;
    }

    public struct TagQueryBuilder64
    {
        public TagQueryBitmask64 Query;

        public static TagQueryBuilder64 Create() => new TagQueryBuilder64();

        public TagQueryBuilder64 Require(TagBitmask64 closureMask)
        {
            Query.RequiredMask = Query.RequiredMask | closureMask;
            return this;
        }

        public TagQueryBuilder64 Block(TagBitmask64 ownMask)
        {
            Query.BlockedMask = Query.BlockedMask | ownMask;
            return this;
        }

        public TagQueryBuilder64 RequireAny(TagBitmask64 closureMask)
        {
            Query.AnyRequiredMask = Query.AnyRequiredMask | closureMask;
            return this;
        }

        public TagQueryBitmask64 Build() => Query;
    }

    public struct TagQueryBuilder128
    {
        public TagQueryBitmask128 Query;

        public static TagQueryBuilder128 Create() => new TagQueryBuilder128();

        public TagQueryBuilder128 Require(TagBitmask128 closureMask)
        {
            Query.RequiredMask = Query.RequiredMask | closureMask;
            return this;
        }

        public TagQueryBuilder128 Block(TagBitmask128 ownMask)
        {
            Query.BlockedMask = Query.BlockedMask | ownMask;
            return this;
        }

        public TagQueryBuilder128 RequireAny(TagBitmask128 closureMask)
        {
            Query.AnyRequiredMask = Query.AnyRequiredMask | closureMask;
            return this;
        }

        public TagQueryBitmask128 Build() => Query;
    }

    public struct TagQueryBuilder256
    {
        public TagQueryBitmask256 Query;

        public static TagQueryBuilder256 Create() => new TagQueryBuilder256();

        public TagQueryBuilder256 Require(TagBitmask256 closureMask)
        {
            Query.RequiredMask = Query.RequiredMask | closureMask;
            return this;
        }

        public TagQueryBuilder256 Block(TagBitmask256 ownMask)
        {
            Query.BlockedMask = Query.BlockedMask | ownMask;
            return this;
        }

        public TagQueryBuilder256 RequireAny(TagBitmask256 closureMask)
        {
            Query.AnyRequiredMask = Query.AnyRequiredMask | closureMask;
            return this;
        }

        public TagQueryBitmask256 Build() => Query;
    }
}
