using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 비트마스크 크기 열거형.
    /// 게임 규모에 따라 적절한 크기를 선택한다.
    /// - Small (32비트): 최대 32개 태그, 가장 빠름
    /// - Medium (64비트): 최대 64개 태그
    /// - Large (128비트): 최대 128개 태그
    /// - Huge (256비트): 최대 256개 태그, 대규모 게임용
    /// </summary>
    public enum TagBitmaskSize
    {
        Small = 32,
        Medium = 64,
        Large = 128,
        Huge = 256
    }

    #region TagBitmask32

    /// <summary>
    /// 32비트 태그 비트마스크.
    /// 소규모 게임용 (최대 32개 태그).
    /// </summary>
    [BurstCompile]
    public struct TagBitmask32 : IEquatable<TagBitmask32>
    {
        public uint Bits;

        public static TagBitmask32 Empty => new TagBitmask32 { Bits = 0 };
        public static TagBitmask32 All => new TagBitmask32 { Bits = uint.MaxValue };

        public const int MaxBits = 32;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask32 FromBitIndex(int index)
        {
            return new TagBitmask32 { Bits = 1u << index };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int index)
        {
            Bits |= 1u << index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearBit(int index)
        {
            Bits &= ~(1u << index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasBit(int index)
        {
            return (Bits & (1u << index)) != 0;
        }

        /// <summary>other의 모든 비트를 포함하는지 검사 (HasAll)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAll(TagBitmask32 other)
        {
            return (Bits & other.Bits) == other.Bits;
        }

        /// <summary>other의 비트 중 하나라도 포함하는지 검사 (HasAny)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAny(TagBitmask32 other)
        {
            return (Bits & other.Bits) != 0;
        }

        /// <summary>other와 겹치는 비트가 없는지 검사 (HasNone)</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsNone(TagBitmask32 other)
        {
            return (Bits & other.Bits) == 0;
        }

        /// <summary>정확히 일치하는지 검사</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TagBitmask32 other)
        {
            return Bits == other.Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return Bits == 0;
        }

        /// <summary>설정된 비트 수 반환</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int PopCount()
        {
            return math.countbits(Bits);
        }

        // 비트 연산자
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask32 operator |(TagBitmask32 a, TagBitmask32 b)
        {
            return new TagBitmask32 { Bits = a.Bits | b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask32 operator &(TagBitmask32 a, TagBitmask32 b)
        {
            return new TagBitmask32 { Bits = a.Bits & b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask32 operator ^(TagBitmask32 a, TagBitmask32 b)
        {
            return new TagBitmask32 { Bits = a.Bits ^ b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask32 operator ~(TagBitmask32 a)
        {
            return new TagBitmask32 { Bits = ~a.Bits };
        }

        public override readonly bool Equals(object obj)
        {
            return obj is TagBitmask32 other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return (int)Bits;
        }

        public static bool operator ==(TagBitmask32 left, TagBitmask32 right) => left.Equals(right);
        public static bool operator !=(TagBitmask32 left, TagBitmask32 right) => !left.Equals(right);
    }

    #endregion

    #region TagBitmask64

    /// <summary>
    /// 64비트 태그 비트마스크.
    /// 중규모 게임용 (최대 64개 태그).
    /// </summary>
    [BurstCompile]
    public struct TagBitmask64 : IEquatable<TagBitmask64>
    {
        public ulong Bits;

        public static TagBitmask64 Empty => new TagBitmask64 { Bits = 0 };
        public static TagBitmask64 All => new TagBitmask64 { Bits = ulong.MaxValue };

        public const int MaxBits = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask64 FromBitIndex(int index)
        {
            return new TagBitmask64 { Bits = 1ul << index };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int index)
        {
            Bits |= 1ul << index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearBit(int index)
        {
            Bits &= ~(1ul << index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasBit(int index)
        {
            return (Bits & (1ul << index)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAll(TagBitmask64 other)
        {
            return (Bits & other.Bits) == other.Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAny(TagBitmask64 other)
        {
            return (Bits & other.Bits) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsNone(TagBitmask64 other)
        {
            return (Bits & other.Bits) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TagBitmask64 other)
        {
            return Bits == other.Bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return Bits == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int PopCount()
        {
            return math.countbits(Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask64 operator |(TagBitmask64 a, TagBitmask64 b)
        {
            return new TagBitmask64 { Bits = a.Bits | b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask64 operator &(TagBitmask64 a, TagBitmask64 b)
        {
            return new TagBitmask64 { Bits = a.Bits & b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask64 operator ^(TagBitmask64 a, TagBitmask64 b)
        {
            return new TagBitmask64 { Bits = a.Bits ^ b.Bits };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask64 operator ~(TagBitmask64 a)
        {
            return new TagBitmask64 { Bits = ~a.Bits };
        }

        public override readonly bool Equals(object obj)
        {
            return obj is TagBitmask64 other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return Bits.GetHashCode();
        }

        public static bool operator ==(TagBitmask64 left, TagBitmask64 right) => left.Equals(right);
        public static bool operator !=(TagBitmask64 left, TagBitmask64 right) => !left.Equals(right);
    }

    #endregion

    #region TagBitmask128

    /// <summary>
    /// 128비트 태그 비트마스크.
    /// 대규모 게임용 (최대 128개 태그).
    /// </summary>
    [BurstCompile]
    public struct TagBitmask128 : IEquatable<TagBitmask128>
    {
        public ulong BitsLow;
        public ulong BitsHigh;

        public static TagBitmask128 Empty => new TagBitmask128 { BitsLow = 0, BitsHigh = 0 };
        public static TagBitmask128 All => new TagBitmask128 { BitsLow = ulong.MaxValue, BitsHigh = ulong.MaxValue };

        public const int MaxBits = 128;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask128 FromBitIndex(int index)
        {
            var result = new TagBitmask128();
            if (index < 64)
                result.BitsLow = 1ul << index;
            else
                result.BitsHigh = 1ul << (index - 64);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int index)
        {
            if (index < 64)
                BitsLow |= 1ul << index;
            else
                BitsHigh |= 1ul << (index - 64);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearBit(int index)
        {
            if (index < 64)
                BitsLow &= ~(1ul << index);
            else
                BitsHigh &= ~(1ul << (index - 64));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasBit(int index)
        {
            if (index < 64)
                return (BitsLow & (1ul << index)) != 0;
            else
                return (BitsHigh & (1ul << (index - 64))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAll(TagBitmask128 other)
        {
            return (BitsLow & other.BitsLow) == other.BitsLow &&
                   (BitsHigh & other.BitsHigh) == other.BitsHigh;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAny(TagBitmask128 other)
        {
            return (BitsLow & other.BitsLow) != 0 ||
                   (BitsHigh & other.BitsHigh) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsNone(TagBitmask128 other)
        {
            return (BitsLow & other.BitsLow) == 0 &&
                   (BitsHigh & other.BitsHigh) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TagBitmask128 other)
        {
            return BitsLow == other.BitsLow && BitsHigh == other.BitsHigh;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return BitsLow == 0 && BitsHigh == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int PopCount()
        {
            return math.countbits(BitsLow) + math.countbits(BitsHigh);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask128 operator |(TagBitmask128 a, TagBitmask128 b)
        {
            return new TagBitmask128 { BitsLow = a.BitsLow | b.BitsLow, BitsHigh = a.BitsHigh | b.BitsHigh };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask128 operator &(TagBitmask128 a, TagBitmask128 b)
        {
            return new TagBitmask128 { BitsLow = a.BitsLow & b.BitsLow, BitsHigh = a.BitsHigh & b.BitsHigh };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask128 operator ^(TagBitmask128 a, TagBitmask128 b)
        {
            return new TagBitmask128 { BitsLow = a.BitsLow ^ b.BitsLow, BitsHigh = a.BitsHigh ^ b.BitsHigh };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask128 operator ~(TagBitmask128 a)
        {
            return new TagBitmask128 { BitsLow = ~a.BitsLow, BitsHigh = ~a.BitsHigh };
        }

        public override readonly bool Equals(object obj)
        {
            return obj is TagBitmask128 other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(BitsLow, BitsHigh);
        }

        public static bool operator ==(TagBitmask128 left, TagBitmask128 right) => left.Equals(right);
        public static bool operator !=(TagBitmask128 left, TagBitmask128 right) => !left.Equals(right);
    }

    #endregion

    #region TagBitmask256

    /// <summary>
    /// 256비트 태그 비트마스크.
    /// 초대규모 게임용 (최대 256개 태그).
    /// </summary>
    [BurstCompile]
    public struct TagBitmask256 : IEquatable<TagBitmask256>
    {
        public ulong Bits0;
        public ulong Bits1;
        public ulong Bits2;
        public ulong Bits3;

        public static TagBitmask256 Empty => new TagBitmask256 { Bits0 = 0, Bits1 = 0, Bits2 = 0, Bits3 = 0 };
        public static TagBitmask256 All => new TagBitmask256 { Bits0 = ulong.MaxValue, Bits1 = ulong.MaxValue, Bits2 = ulong.MaxValue, Bits3 = ulong.MaxValue };

        public const int MaxBits = 256;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask256 FromBitIndex(int index)
        {
            var result = new TagBitmask256();
            int segment = index / 64;
            int bit = index % 64;
            switch (segment)
            {
                case 0: result.Bits0 = 1ul << bit; break;
                case 1: result.Bits1 = 1ul << bit; break;
                case 2: result.Bits2 = 1ul << bit; break;
                case 3: result.Bits3 = 1ul << bit; break;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int index)
        {
            int segment = index / 64;
            int bit = index % 64;
            switch (segment)
            {
                case 0: Bits0 |= 1ul << bit; break;
                case 1: Bits1 |= 1ul << bit; break;
                case 2: Bits2 |= 1ul << bit; break;
                case 3: Bits3 |= 1ul << bit; break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearBit(int index)
        {
            int segment = index / 64;
            int bit = index % 64;
            switch (segment)
            {
                case 0: Bits0 &= ~(1ul << bit); break;
                case 1: Bits1 &= ~(1ul << bit); break;
                case 2: Bits2 &= ~(1ul << bit); break;
                case 3: Bits3 &= ~(1ul << bit); break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasBit(int index)
        {
            int segment = index / 64;
            int bit = index % 64;
            return segment switch
            {
                0 => (Bits0 & (1ul << bit)) != 0,
                1 => (Bits1 & (1ul << bit)) != 0,
                2 => (Bits2 & (1ul << bit)) != 0,
                3 => (Bits3 & (1ul << bit)) != 0,
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAll(TagBitmask256 other)
        {
            return (Bits0 & other.Bits0) == other.Bits0 &&
                   (Bits1 & other.Bits1) == other.Bits1 &&
                   (Bits2 & other.Bits2) == other.Bits2 &&
                   (Bits3 & other.Bits3) == other.Bits3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsAny(TagBitmask256 other)
        {
            return (Bits0 & other.Bits0) != 0 ||
                   (Bits1 & other.Bits1) != 0 ||
                   (Bits2 & other.Bits2) != 0 ||
                   (Bits3 & other.Bits3) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsNone(TagBitmask256 other)
        {
            return (Bits0 & other.Bits0) == 0 &&
                   (Bits1 & other.Bits1) == 0 &&
                   (Bits2 & other.Bits2) == 0 &&
                   (Bits3 & other.Bits3) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(TagBitmask256 other)
        {
            return Bits0 == other.Bits0 && Bits1 == other.Bits1 &&
                   Bits2 == other.Bits2 && Bits3 == other.Bits3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsEmpty()
        {
            return Bits0 == 0 && Bits1 == 0 && Bits2 == 0 && Bits3 == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int PopCount()
        {
            return math.countbits(Bits0) + math.countbits(Bits1) +
                   math.countbits(Bits2) + math.countbits(Bits3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask256 operator |(TagBitmask256 a, TagBitmask256 b)
        {
            return new TagBitmask256
            {
                Bits0 = a.Bits0 | b.Bits0,
                Bits1 = a.Bits1 | b.Bits1,
                Bits2 = a.Bits2 | b.Bits2,
                Bits3 = a.Bits3 | b.Bits3
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask256 operator &(TagBitmask256 a, TagBitmask256 b)
        {
            return new TagBitmask256
            {
                Bits0 = a.Bits0 & b.Bits0,
                Bits1 = a.Bits1 & b.Bits1,
                Bits2 = a.Bits2 & b.Bits2,
                Bits3 = a.Bits3 & b.Bits3
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask256 operator ^(TagBitmask256 a, TagBitmask256 b)
        {
            return new TagBitmask256
            {
                Bits0 = a.Bits0 ^ b.Bits0,
                Bits1 = a.Bits1 ^ b.Bits1,
                Bits2 = a.Bits2 ^ b.Bits2,
                Bits3 = a.Bits3 ^ b.Bits3
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagBitmask256 operator ~(TagBitmask256 a)
        {
            return new TagBitmask256
            {
                Bits0 = ~a.Bits0,
                Bits1 = ~a.Bits1,
                Bits2 = ~a.Bits2,
                Bits3 = ~a.Bits3
            };
        }

        public override readonly bool Equals(object obj)
        {
            return obj is TagBitmask256 other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Bits0, Bits1, Bits2, Bits3);
        }

        public static bool operator ==(TagBitmask256 left, TagBitmask256 right) => left.Equals(right);
        public static bool operator !=(TagBitmask256 left, TagBitmask256 right) => !left.Equals(right);
    }

    #endregion

    #region TagBitmask Utility

    /// <summary>
    /// 비트마스크 유틸리티 함수들.
    /// </summary>
    [BurstCompile]
    public static class TagBitmaskUtility
    {
        /// <summary>
        /// 비트마스크 크기에 따른 최대 태그 수 반환.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxTagCount(TagBitmaskSize size)
        {
            return (int)size;
        }

        /// <summary>
        /// 비트 인덱스가 해당 크기에 유효한지 확인.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidBitIndex(int index, TagBitmaskSize size)
        {
            return index >= 0 && index < (int)size;
        }
    }

    #endregion
}
