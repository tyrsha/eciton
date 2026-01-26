using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 태그 정의 구조체.
    /// Parent Closure 방식으로, 각 태그는 자신과 모든 부모의 비트를 포함한다.
    ///
    /// 예: "Status.Debuff.Stunned" 태그 계층
    /// - Status (bit 0)           : ClosureMask = 0b001
    /// - Status.Debuff (bit 1)    : ClosureMask = 0b011 (자신 + Status)
    /// - Status.Debuff.Stunned (bit 2) : ClosureMask = 0b111 (자신 + Debuff + Status)
    ///
    /// 이 방식의 장점:
    /// 1. HasTag("Status.Debuff") 쿼리시 Stunned, Slowed 등 모든 자식도 매칭
    /// 2. 비트 연산만으로 O(1) 계층 쿼리 가능
    /// 3. Burst 호환
    /// </summary>
    [BurstCompile]
    public struct TagDefinition32 : IEquatable<TagDefinition32>
    {
        /// <summary>태그의 고유 식별자 (해시 또는 ID)</summary>
        public int TagId;

        /// <summary>비트마스크 내 비트 인덱스 (0-31)</summary>
        public int BitIndex;

        /// <summary>부모 태그의 TagId (-1이면 루트)</summary>
        public int ParentTagId;

        /// <summary>자신의 비트만 포함하는 마스크</summary>
        public TagBitmask32 OwnMask;

        /// <summary>자신과 모든 조상의 비트를 포함하는 마스크 (Parent Closure)</summary>
        public TagBitmask32 ClosureMask;

        public static TagDefinition32 Invalid => new TagDefinition32 { TagId = 0, BitIndex = -1, ParentTagId = -1 };
        public bool IsValid => TagId != 0 && BitIndex >= 0;

        /// <summary>루트 태그인지 확인</summary>
        public bool IsRoot => ParentTagId < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TagDefinition32 other)
        {
            return TagId == other.TagId;
        }

        public override bool Equals(object obj)
        {
            return obj is TagDefinition32 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagId;
        }
    }

    [BurstCompile]
    public struct TagDefinition64 : IEquatable<TagDefinition64>
    {
        public int TagId;
        public int BitIndex;
        public int ParentTagId;
        public TagBitmask64 OwnMask;
        public TagBitmask64 ClosureMask;

        public static TagDefinition64 Invalid => new TagDefinition64 { TagId = 0, BitIndex = -1, ParentTagId = -1 };
        public bool IsValid => TagId != 0 && BitIndex >= 0;
        public bool IsRoot => ParentTagId < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TagDefinition64 other)
        {
            return TagId == other.TagId;
        }

        public override bool Equals(object obj)
        {
            return obj is TagDefinition64 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagId;
        }
    }

    [BurstCompile]
    public struct TagDefinition128 : IEquatable<TagDefinition128>
    {
        public int TagId;
        public int BitIndex;
        public int ParentTagId;
        public TagBitmask128 OwnMask;
        public TagBitmask128 ClosureMask;

        public static TagDefinition128 Invalid => new TagDefinition128 { TagId = 0, BitIndex = -1, ParentTagId = -1 };
        public bool IsValid => TagId != 0 && BitIndex >= 0;
        public bool IsRoot => ParentTagId < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TagDefinition128 other)
        {
            return TagId == other.TagId;
        }

        public override bool Equals(object obj)
        {
            return obj is TagDefinition128 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagId;
        }
    }

    [BurstCompile]
    public struct TagDefinition256 : IEquatable<TagDefinition256>
    {
        public int TagId;
        public int BitIndex;
        public int ParentTagId;
        public TagBitmask256 OwnMask;
        public TagBitmask256 ClosureMask;

        public static TagDefinition256 Invalid => new TagDefinition256 { TagId = 0, BitIndex = -1, ParentTagId = -1 };
        public bool IsValid => TagId != 0 && BitIndex >= 0;
        public bool IsRoot => ParentTagId < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TagDefinition256 other)
        {
            return TagId == other.TagId;
        }

        public override bool Equals(object obj)
        {
            return obj is TagDefinition256 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagId;
        }
    }

    #region Tag Handle

    /// <summary>
    /// 런타임에서 태그를 참조하는 핸들.
    /// TagId와 BitIndex를 함께 저장하여 빠른 비트 연산을 지원한다.
    /// </summary>
    [BurstCompile]
    public struct TagHandle : IEquatable<TagHandle>
    {
        /// <summary>태그의 고유 식별자</summary>
        public int TagId;

        /// <summary>비트마스크 내 비트 인덱스</summary>
        public int BitIndex;

        public static TagHandle Invalid => new TagHandle { TagId = 0, BitIndex = -1 };
        public bool IsValid => TagId != 0 && BitIndex >= 0;

        public TagHandle(int tagId, int bitIndex)
        {
            TagId = tagId;
            BitIndex = bitIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TagHandle other)
        {
            return TagId == other.TagId;
        }

        public override bool Equals(object obj)
        {
            return obj is TagHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TagId;
        }

        public static bool operator ==(TagHandle left, TagHandle right) => left.Equals(right);
        public static bool operator !=(TagHandle left, TagHandle right) => !left.Equals(right);
    }

    #endregion

    #region Tag Query Structures

    /// <summary>
    /// 비트마스크 기반 태그 쿼리 (32비트).
    /// RequiredMask: 이 비트들이 모두 있어야 함 (AND)
    /// BlockedMask: 이 비트들이 하나라도 있으면 안됨 (NAND)
    /// AnyRequiredMask: 이 비트들 중 하나라도 있어야 함 (OR)
    /// </summary>
    [BurstCompile]
    public struct TagQueryBitmask32
    {
        /// <summary>모두 필요한 태그들의 closure mask OR</summary>
        public TagBitmask32 RequiredMask;

        /// <summary>하나라도 있으면 안되는 태그들의 own mask OR</summary>
        public TagBitmask32 BlockedMask;

        /// <summary>하나라도 필요한 태그들의 closure mask OR (optional)</summary>
        public TagBitmask32 AnyRequiredMask;

        public static TagQueryBitmask32 Empty => new TagQueryBitmask32();

        /// <summary>
        /// 주어진 태그 컨테이너가 이 쿼리를 만족하는지 검사.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Matches(TagBitmask32 containerMask)
        {
            // Blocked 태그가 있으면 실패
            if (!containerMask.ContainsNone(BlockedMask))
                return false;

            // Required 태그가 모두 있어야 함
            if (!containerMask.ContainsAll(RequiredMask))
                return false;

            // AnyRequired가 설정되어 있으면 하나라도 있어야 함
            if (!AnyRequiredMask.IsEmpty() && !containerMask.ContainsAny(AnyRequiredMask))
                return false;

            return true;
        }
    }

    [BurstCompile]
    public struct TagQueryBitmask64
    {
        public TagBitmask64 RequiredMask;
        public TagBitmask64 BlockedMask;
        public TagBitmask64 AnyRequiredMask;

        public static TagQueryBitmask64 Empty => new TagQueryBitmask64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Matches(TagBitmask64 containerMask)
        {
            if (!containerMask.ContainsNone(BlockedMask))
                return false;
            if (!containerMask.ContainsAll(RequiredMask))
                return false;
            if (!AnyRequiredMask.IsEmpty() && !containerMask.ContainsAny(AnyRequiredMask))
                return false;
            return true;
        }
    }

    [BurstCompile]
    public struct TagQueryBitmask128
    {
        public TagBitmask128 RequiredMask;
        public TagBitmask128 BlockedMask;
        public TagBitmask128 AnyRequiredMask;

        public static TagQueryBitmask128 Empty => new TagQueryBitmask128();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Matches(TagBitmask128 containerMask)
        {
            if (!containerMask.ContainsNone(BlockedMask))
                return false;
            if (!containerMask.ContainsAll(RequiredMask))
                return false;
            if (!AnyRequiredMask.IsEmpty() && !containerMask.ContainsAny(AnyRequiredMask))
                return false;
            return true;
        }
    }

    [BurstCompile]
    public struct TagQueryBitmask256
    {
        public TagBitmask256 RequiredMask;
        public TagBitmask256 BlockedMask;
        public TagBitmask256 AnyRequiredMask;

        public static TagQueryBitmask256 Empty => new TagQueryBitmask256();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Matches(TagBitmask256 containerMask)
        {
            if (!containerMask.ContainsNone(BlockedMask))
                return false;
            if (!containerMask.ContainsAll(RequiredMask))
                return false;
            if (!AnyRequiredMask.IsEmpty() && !containerMask.ContainsAny(AnyRequiredMask))
                return false;
            return true;
        }
    }

    #endregion
}
