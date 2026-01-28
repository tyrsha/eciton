using NUnit.Framework;

namespace Tyrsha.Eciton.Tests
{
    /// <summary>
    /// TagBitmask 비트 연산 단위 테스트.
    /// </summary>
    public class TagBitmaskTests
    {
        #region TagBitmask32 Tests

        [Test]
        public void TagBitmask32_FromBitIndex_sets_correct_bit()
        {
            var mask0 = TagBitmask32.FromBitIndex(0);
            var mask5 = TagBitmask32.FromBitIndex(5);
            var mask31 = TagBitmask32.FromBitIndex(31);

            Assert.AreEqual(1u, mask0.Bits);
            Assert.AreEqual(32u, mask5.Bits); // 2^5 = 32
            Assert.AreEqual(0x80000000u, mask31.Bits);
        }

        [Test]
        public void TagBitmask32_SetBit_and_ClearBit_work_correctly()
        {
            var mask = TagBitmask32.Empty;

            mask.SetBit(3);
            Assert.IsTrue(mask.HasBit(3));
            Assert.IsFalse(mask.HasBit(2));

            mask.SetBit(7);
            Assert.IsTrue(mask.HasBit(3));
            Assert.IsTrue(mask.HasBit(7));

            mask.ClearBit(3);
            Assert.IsFalse(mask.HasBit(3));
            Assert.IsTrue(mask.HasBit(7));
        }

        [Test]
        public void TagBitmask32_ContainsAll_returns_true_when_all_bits_present()
        {
            var container = new TagBitmask32 { Bits = 0b11111 }; // bits 0-4
            var query1 = new TagBitmask32 { Bits = 0b00011 };    // bits 0-1
            var query2 = new TagBitmask32 { Bits = 0b11111 };    // bits 0-4
            var query3 = new TagBitmask32 { Bits = 0b100000 };   // bit 5

            Assert.IsTrue(container.ContainsAll(query1));
            Assert.IsTrue(container.ContainsAll(query2));
            Assert.IsFalse(container.ContainsAll(query3));
        }

        [Test]
        public void TagBitmask32_ContainsAny_returns_true_when_any_bit_present()
        {
            var container = new TagBitmask32 { Bits = 0b10100 }; // bits 2, 4
            var query1 = new TagBitmask32 { Bits = 0b00100 };    // bit 2
            var query2 = new TagBitmask32 { Bits = 0b01011 };    // bits 0, 1, 3
            var query3 = new TagBitmask32 { Bits = 0b10001 };    // bits 0, 4

            Assert.IsTrue(container.ContainsAny(query1));
            Assert.IsFalse(container.ContainsAny(query2));
            Assert.IsTrue(container.ContainsAny(query3));
        }

        [Test]
        public void TagBitmask32_ContainsNone_returns_true_when_no_overlap()
        {
            var container = new TagBitmask32 { Bits = 0b11000 }; // bits 3, 4
            var query1 = new TagBitmask32 { Bits = 0b00111 };    // bits 0-2
            var query2 = new TagBitmask32 { Bits = 0b01000 };    // bit 3

            Assert.IsTrue(container.ContainsNone(query1));
            Assert.IsFalse(container.ContainsNone(query2));
        }

        [Test]
        public void TagBitmask32_PopCount_returns_correct_count()
        {
            Assert.AreEqual(0, TagBitmask32.Empty.PopCount());
            Assert.AreEqual(32, TagBitmask32.All.PopCount());
            Assert.AreEqual(3, new TagBitmask32 { Bits = 0b10101 }.PopCount());
        }

        [Test]
        public void TagBitmask32_operators_work_correctly()
        {
            var a = new TagBitmask32 { Bits = 0b1100 };
            var b = new TagBitmask32 { Bits = 0b1010 };

            Assert.AreEqual(0b1110u, (a | b).Bits); // OR
            Assert.AreEqual(0b1000u, (a & b).Bits); // AND
            Assert.AreEqual(0b0110u, (a ^ b).Bits); // XOR
        }

        #endregion

        #region TagBitmask64 Tests

        [Test]
        public void TagBitmask64_handles_bits_above_32()
        {
            var mask32 = TagBitmask64.FromBitIndex(32);
            var mask63 = TagBitmask64.FromBitIndex(63);

            Assert.AreEqual(0x100000000ul, mask32.Bits);
            Assert.AreEqual(0x8000000000000000ul, mask63.Bits);

            Assert.IsTrue(mask32.HasBit(32));
            Assert.IsFalse(mask32.HasBit(31));
            Assert.IsTrue(mask63.HasBit(63));
        }

        [Test]
        public void TagBitmask64_ContainsAll_works_across_full_range()
        {
            var container = new TagBitmask64 { Bits = 0xFFFFFFFFFFFFFFFFul };
            var query = TagBitmask64.FromBitIndex(63) | TagBitmask64.FromBitIndex(0);

            Assert.IsTrue(container.ContainsAll(query));
        }

        #endregion

        #region TagBitmask128 Tests

        [Test]
        public void TagBitmask128_handles_bits_in_both_halves()
        {
            var mask0 = TagBitmask128.FromBitIndex(0);
            var mask63 = TagBitmask128.FromBitIndex(63);
            var mask64 = TagBitmask128.FromBitIndex(64);
            var mask127 = TagBitmask128.FromBitIndex(127);

            Assert.AreEqual(1ul, mask0.BitsLow);
            Assert.AreEqual(0ul, mask0.BitsHigh);

            Assert.AreEqual(0x8000000000000000ul, mask63.BitsLow);
            Assert.AreEqual(0ul, mask63.BitsHigh);

            Assert.AreEqual(0ul, mask64.BitsLow);
            Assert.AreEqual(1ul, mask64.BitsHigh);

            Assert.AreEqual(0ul, mask127.BitsLow);
            Assert.AreEqual(0x8000000000000000ul, mask127.BitsHigh);
        }

        [Test]
        public void TagBitmask128_ContainsAll_works_across_halves()
        {
            var container = TagBitmask128.FromBitIndex(10) | TagBitmask128.FromBitIndex(100);
            var query1 = TagBitmask128.FromBitIndex(10);
            var query2 = TagBitmask128.FromBitIndex(100);
            var queryBoth = query1 | query2;

            Assert.IsTrue(container.ContainsAll(query1));
            Assert.IsTrue(container.ContainsAll(query2));
            Assert.IsTrue(container.ContainsAll(queryBoth));
        }

        #endregion

        #region TagBitmask256 Tests

        [Test]
        public void TagBitmask256_handles_all_four_segments()
        {
            var mask0 = TagBitmask256.FromBitIndex(0);
            var mask64 = TagBitmask256.FromBitIndex(64);
            var mask128 = TagBitmask256.FromBitIndex(128);
            var mask192 = TagBitmask256.FromBitIndex(192);
            var mask255 = TagBitmask256.FromBitIndex(255);

            Assert.IsTrue(mask0.HasBit(0));
            Assert.IsTrue(mask64.HasBit(64));
            Assert.IsTrue(mask128.HasBit(128));
            Assert.IsTrue(mask192.HasBit(192));
            Assert.IsTrue(mask255.HasBit(255));

            Assert.AreEqual(1ul, mask0.Bits0);
            Assert.AreEqual(1ul, mask64.Bits1);
            Assert.AreEqual(1ul, mask128.Bits2);
            Assert.AreEqual(1ul, mask192.Bits3);
        }

        [Test]
        public void TagBitmask256_ContainsAny_works_across_segments()
        {
            var container = TagBitmask256.FromBitIndex(50) | TagBitmask256.FromBitIndex(150);
            var query1 = TagBitmask256.FromBitIndex(50);
            var query2 = TagBitmask256.FromBitIndex(200);
            var query3 = TagBitmask256.FromBitIndex(150);

            Assert.IsTrue(container.ContainsAny(query1));
            Assert.IsFalse(container.ContainsAny(query2));
            Assert.IsTrue(container.ContainsAny(query3));
        }

        [Test]
        public void TagBitmask256_PopCount_returns_correct_count()
        {
            var mask = TagBitmask256.FromBitIndex(0) |
                       TagBitmask256.FromBitIndex(64) |
                       TagBitmask256.FromBitIndex(128) |
                       TagBitmask256.FromBitIndex(192);

            Assert.AreEqual(4, mask.PopCount());
        }

        #endregion
    }
}
