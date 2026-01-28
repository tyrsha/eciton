using NUnit.Framework;

namespace Tyrsha.Eciton.Tests
{
    /// <summary>
    /// TagQuery 매칭 로직 테스트.
    /// Required (AND), Blocked (NAND), AnyRequired (OR) 조합 검증.
    /// </summary>
    public class TagQueryTests
    {
        // 테스트용 태그 마스크 정의
        // GAS 스타일 계층 구조 시뮬레이션:
        // - Status (bit 0)
        //   - Status.Buff (bit 1) -> closure = 0,1
        //     - Status.Buff.Shield (bit 2) -> closure = 0,1,2
        //     - Status.Buff.Haste (bit 3) -> closure = 0,1,3
        //   - Status.Debuff (bit 4) -> closure = 0,4
        //     - Status.Debuff.Stunned (bit 5) -> closure = 0,4,5
        //     - Status.Debuff.Slowed (bit 6) -> closure = 0,4,6

        private static readonly TagBitmask32 Status_Own = TagBitmask32.FromBitIndex(0);
        private static readonly TagBitmask32 Buff_Own = TagBitmask32.FromBitIndex(1);
        private static readonly TagBitmask32 Shield_Own = TagBitmask32.FromBitIndex(2);
        private static readonly TagBitmask32 Haste_Own = TagBitmask32.FromBitIndex(3);
        private static readonly TagBitmask32 Debuff_Own = TagBitmask32.FromBitIndex(4);
        private static readonly TagBitmask32 Stunned_Own = TagBitmask32.FromBitIndex(5);
        private static readonly TagBitmask32 Slowed_Own = TagBitmask32.FromBitIndex(6);

        private static readonly TagBitmask32 Status_Closure = Status_Own;
        private static readonly TagBitmask32 Buff_Closure = Status_Own | Buff_Own;
        private static readonly TagBitmask32 Shield_Closure = Status_Own | Buff_Own | Shield_Own;
        private static readonly TagBitmask32 Haste_Closure = Status_Own | Buff_Own | Haste_Own;
        private static readonly TagBitmask32 Debuff_Closure = Status_Own | Debuff_Own;
        private static readonly TagBitmask32 Stunned_Closure = Status_Own | Debuff_Own | Stunned_Own;
        private static readonly TagBitmask32 Slowed_Closure = Status_Own | Debuff_Own | Slowed_Own;

        #region Required (AND) Tests

        [Test]
        public void Query_with_required_matches_when_all_present()
        {
            // 컨테이너: Shield + Haste
            var container = Shield_Closure | Haste_Closure;

            // 쿼리: Buff 필요 (Shield와 Haste 모두 Buff의 자식)
            var query = new TagQueryBitmask32 { RequiredMask = Buff_Closure };

            Assert.IsTrue(query.Matches(container));
        }

        [Test]
        public void Query_with_required_fails_when_missing()
        {
            // 컨테이너: Shield만
            var container = Shield_Closure;

            // 쿼리: Debuff 필요
            var query = new TagQueryBitmask32 { RequiredMask = Debuff_Closure };

            Assert.IsFalse(query.Matches(container));
        }

        [Test]
        public void Query_with_multiple_required_needs_all()
        {
            // 컨테이너: Shield + Stunned
            var container = Shield_Closure | Stunned_Closure;

            // 쿼리: Buff AND Debuff 필요
            var query = new TagQueryBitmask32 { RequiredMask = Buff_Closure | Debuff_Closure };

            Assert.IsTrue(query.Matches(container));

            // Shield만 있으면 실패
            var containerOnlyBuff = Shield_Closure;
            Assert.IsFalse(query.Matches(containerOnlyBuff));
        }

        #endregion

        #region Blocked (NAND) Tests

        [Test]
        public void Query_with_blocked_fails_when_blocked_tag_present()
        {
            // 컨테이너: Stunned
            var container = Stunned_Closure;

            // 쿼리: Stunned 블록
            var query = new TagQueryBitmask32 { BlockedMask = Stunned_Own };

            Assert.IsFalse(query.Matches(container));
        }

        [Test]
        public void Query_with_blocked_passes_when_blocked_tag_absent()
        {
            // 컨테이너: Shield
            var container = Shield_Closure;

            // 쿼리: Stunned 블록 (Shield에는 없음)
            var query = new TagQueryBitmask32 { BlockedMask = Stunned_Own };

            Assert.IsTrue(query.Matches(container));
        }

        [Test]
        public void Query_with_multiple_blocked_fails_if_any_present()
        {
            // 컨테이너: Slowed
            var container = Slowed_Closure;

            // 쿼리: Stunned OR Slowed 블록
            var query = new TagQueryBitmask32 { BlockedMask = Stunned_Own | Slowed_Own };

            Assert.IsFalse(query.Matches(container));
        }

        #endregion

        #region AnyRequired (OR) Tests

        [Test]
        public void Query_with_any_required_passes_if_one_present()
        {
            // 컨테이너: Shield
            var container = Shield_Closure;

            // 쿼리: Shield OR Haste 중 하나 필요
            var query = new TagQueryBitmask32 { AnyRequiredMask = Shield_Closure | Haste_Closure };

            Assert.IsTrue(query.Matches(container));
        }

        [Test]
        public void Query_with_any_required_fails_if_none_present()
        {
            // 컨테이너: Stunned
            var container = Stunned_Closure;

            // 쿼리: Shield OR Haste 중 하나 필요
            var query = new TagQueryBitmask32 { AnyRequiredMask = Shield_Closure | Haste_Closure };

            Assert.IsFalse(query.Matches(container));
        }

        [Test]
        public void Empty_any_required_is_ignored()
        {
            var container = Stunned_Closure;

            // AnyRequired가 비어있으면 항상 통과
            var query = new TagQueryBitmask32
            {
                RequiredMask = TagBitmask32.Empty,
                BlockedMask = TagBitmask32.Empty,
                AnyRequiredMask = TagBitmask32.Empty
            };

            Assert.IsTrue(query.Matches(container));
        }

        #endregion

        #region Combined Query Tests

        [Test]
        public void Combined_query_all_conditions_must_pass()
        {
            // 시나리오: 스킬 사용 조건
            // - Buff 상태여야 함 (Required)
            // - Stunned 상태면 안됨 (Blocked)
            // - Shield OR Haste 중 하나 필요 (AnyRequired)

            var query = new TagQueryBitmask32
            {
                RequiredMask = Buff_Closure,
                BlockedMask = Stunned_Own,
                AnyRequiredMask = Shield_Closure | Haste_Closure
            };

            // 통과: Shield (Buff + Shield, no Stunned)
            var pass1 = Shield_Closure;
            Assert.IsTrue(query.Matches(pass1));

            // 통과: Haste (Buff + Haste, no Stunned)
            var pass2 = Haste_Closure;
            Assert.IsTrue(query.Matches(pass2));

            // 실패: Buff만 (AnyRequired 불충족)
            var fail1 = Buff_Closure;
            Assert.IsFalse(query.Matches(fail1));

            // 실패: Shield + Stunned (Blocked)
            var fail2 = Shield_Closure | Stunned_Closure;
            Assert.IsFalse(query.Matches(fail2));

            // 실패: Stunned만 (Required 불충족)
            var fail3 = Stunned_Closure;
            Assert.IsFalse(query.Matches(fail3));
        }

        #endregion

        #region TagQueryBuilder Tests

        [Test]
        public void TagQueryBuilder_creates_correct_query()
        {
            var query = TagQueryBuilder32.Create()
                .Require(Buff_Closure)
                .Block(Stunned_Own)
                .RequireAny(Shield_Closure | Haste_Closure)
                .Build();

            var container = Shield_Closure;
            Assert.IsTrue(query.Matches(container));

            var blockedContainer = Shield_Closure | Stunned_Closure;
            Assert.IsFalse(query.Matches(blockedContainer));
        }

        [Test]
        public void TagQueryBuilder_can_chain_multiple_requires()
        {
            var query = TagQueryBuilder32.Create()
                .Require(Buff_Closure)
                .Require(Debuff_Closure)
                .Build();

            // 둘 다 있어야 통과
            var pass = Shield_Closure | Stunned_Closure;
            Assert.IsTrue(query.Matches(pass));

            // 하나만 있으면 실패
            var fail = Shield_Closure;
            Assert.IsFalse(query.Matches(fail));
        }

        [Test]
        public void TagQueryBuilder_can_chain_multiple_blocks()
        {
            var query = TagQueryBuilder32.Create()
                .Block(Stunned_Own)
                .Block(Slowed_Own)
                .Build();

            // Stunned도 Slowed도 없으면 통과
            var pass = Shield_Closure;
            Assert.IsTrue(query.Matches(pass));

            // 둘 중 하나라도 있으면 실패
            var fail1 = Stunned_Closure;
            var fail2 = Slowed_Closure;
            Assert.IsFalse(query.Matches(fail1));
            Assert.IsFalse(query.Matches(fail2));
        }

        #endregion

        #region 64/128/256-bit Query Tests

        [Test]
        public void TagQueryBitmask64_matches_correctly()
        {
            var tag40 = TagBitmask64.FromBitIndex(40);
            var tag50 = TagBitmask64.FromBitIndex(50);
            var container = tag40 | tag50;

            var query = new TagQueryBitmask64
            {
                RequiredMask = tag40,
                BlockedMask = TagBitmask64.FromBitIndex(60)
            };

            Assert.IsTrue(query.Matches(container));

            var blockedContainer = container | TagBitmask64.FromBitIndex(60);
            Assert.IsFalse(query.Matches(blockedContainer));
        }

        [Test]
        public void TagQueryBitmask128_matches_across_halves()
        {
            var tagLow = TagBitmask128.FromBitIndex(30);
            var tagHigh = TagBitmask128.FromBitIndex(100);
            var container = tagLow | tagHigh;

            var query = new TagQueryBitmask128
            {
                RequiredMask = tagLow | tagHigh
            };

            Assert.IsTrue(query.Matches(container));

            var partialContainer = tagLow;
            Assert.IsFalse(query.Matches(partialContainer));
        }

        [Test]
        public void TagQueryBitmask256_matches_across_segments()
        {
            var tag0 = TagBitmask256.FromBitIndex(10);
            var tag1 = TagBitmask256.FromBitIndex(70);
            var tag2 = TagBitmask256.FromBitIndex(140);
            var tag3 = TagBitmask256.FromBitIndex(200);

            var container = tag0 | tag1 | tag2 | tag3;

            var query = new TagQueryBitmask256
            {
                RequiredMask = tag0 | tag3,
                AnyRequiredMask = tag1 | tag2
            };

            Assert.IsTrue(query.Matches(container));
        }

        #endregion
    }
}
