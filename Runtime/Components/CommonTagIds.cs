namespace Tyrsha.Eciton
{
    /// <summary>
    /// GAS 스타일 태그 계층 구조 예제.
    /// 실제 프로젝트에서는 TagDefinitionAsset으로 에디터에서 정의하는 것을 권장.
    ///
    /// 계층 예시:
    /// - Status (0)
    ///   - Status.Buff (1)
    ///     - Status.Buff.Shield (2)
    ///     - Status.Buff.Haste (3)
    ///     - Status.Buff.Regen (4)
    ///   - Status.Debuff (5)
    ///     - Status.Debuff.Stunned (6)
    ///     - Status.Debuff.Slowed (7)
    ///     - Status.Debuff.Burning (8)
    ///     - Status.Debuff.Poisoned (9)
    /// - Ability (10)
    ///   - Ability.Cooldown (11)
    ///     - Ability.Cooldown.Fireball (12)
    ///     - Ability.Cooldown.Heal (13)
    ///     - Ability.Cooldown.StunBolt (14)
    ///   - Ability.Casting (15)
    ///   - Ability.Channeling (16)
    /// - State (17)
    ///   - State.Dead (18)
    ///   - State.Invulnerable (19)
    ///   - State.Silenced (20)
    /// - Damage (21)
    ///   - Damage.Fire (22)
    ///   - Damage.Ice (23)
    ///   - Damage.Physical (24)
    ///   - Damage.Poison (25)
    /// </summary>
    public static class CommonTagIds
    {
        // Status 계열 (부모 closure 포함)
        public const int Status = 1;
        public const int Status_Buff = 2;
        public const int Status_Buff_Shield = 3;
        public const int Status_Buff_Haste = 4;
        public const int Status_Buff_Regen = 5;
        public const int Status_Debuff = 6;
        public const int Status_Debuff_Stunned = 7;
        public const int Status_Debuff_Slowed = 8;
        public const int Status_Debuff_Burning = 9;
        public const int Status_Debuff_Poisoned = 10;

        // Ability 계열
        public const int Ability = 11;
        public const int Ability_Cooldown = 12;
        public const int Ability_Cooldown_Fireball = 13;
        public const int Ability_Cooldown_Heal = 14;
        public const int Ability_Cooldown_StunBolt = 15;
        public const int Ability_Cooldown_Cleanse = 16;
        public const int Ability_Casting = 17;
        public const int Ability_Channeling = 18;

        // State 계열
        public const int State = 19;
        public const int State_Dead = 20;
        public const int State_Invulnerable = 21;
        public const int State_Silenced = 22;
        public const int State_Rooted = 23;

        // Damage 계열
        public const int Damage = 24;
        public const int Damage_Fire = 25;
        public const int Damage_Ice = 26;
        public const int Damage_Physical = 27;
        public const int Damage_Poison = 28;
    }

    /// <summary>
    /// 예제용 32비트 비트마스크 정적 정의.
    /// 실제 프로젝트에서는 TagDatabase에서 동적으로 생성된다.
    /// </summary>
    public static class ExampleTagMasks32
    {
        // 비트 인덱스 (0-based, 위상정렬 순서)
        // Status (bit 0), Status.Buff (bit 1), Status.Buff.Shield (bit 2), ...

        // OwnMask: 해당 태그만의 비트
        public static readonly TagBitmask32 Status_Own = TagBitmask32.FromBitIndex(0);
        public static readonly TagBitmask32 Status_Buff_Own = TagBitmask32.FromBitIndex(1);
        public static readonly TagBitmask32 Status_Buff_Shield_Own = TagBitmask32.FromBitIndex(2);
        public static readonly TagBitmask32 Status_Buff_Haste_Own = TagBitmask32.FromBitIndex(3);
        public static readonly TagBitmask32 Status_Buff_Regen_Own = TagBitmask32.FromBitIndex(4);
        public static readonly TagBitmask32 Status_Debuff_Own = TagBitmask32.FromBitIndex(5);
        public static readonly TagBitmask32 Status_Debuff_Stunned_Own = TagBitmask32.FromBitIndex(6);
        public static readonly TagBitmask32 Status_Debuff_Slowed_Own = TagBitmask32.FromBitIndex(7);
        public static readonly TagBitmask32 Status_Debuff_Burning_Own = TagBitmask32.FromBitIndex(8);
        public static readonly TagBitmask32 Status_Debuff_Poisoned_Own = TagBitmask32.FromBitIndex(9);

        // ClosureMask: 자신 + 모든 부모 비트
        // Status closure = Status own
        public static readonly TagBitmask32 Status_Closure = Status_Own;

        // Status.Buff closure = Status.Buff own | Status own
        public static readonly TagBitmask32 Status_Buff_Closure = Status_Buff_Own | Status_Own;

        // Status.Buff.Shield closure = Shield own | Buff own | Status own
        public static readonly TagBitmask32 Status_Buff_Shield_Closure = Status_Buff_Shield_Own | Status_Buff_Own | Status_Own;
        public static readonly TagBitmask32 Status_Buff_Haste_Closure = Status_Buff_Haste_Own | Status_Buff_Own | Status_Own;
        public static readonly TagBitmask32 Status_Buff_Regen_Closure = Status_Buff_Regen_Own | Status_Buff_Own | Status_Own;

        // Status.Debuff closure = Debuff own | Status own
        public static readonly TagBitmask32 Status_Debuff_Closure = Status_Debuff_Own | Status_Own;

        // Status.Debuff.* closure = * own | Debuff own | Status own
        public static readonly TagBitmask32 Status_Debuff_Stunned_Closure = Status_Debuff_Stunned_Own | Status_Debuff_Own | Status_Own;
        public static readonly TagBitmask32 Status_Debuff_Slowed_Closure = Status_Debuff_Slowed_Own | Status_Debuff_Own | Status_Own;
        public static readonly TagBitmask32 Status_Debuff_Burning_Closure = Status_Debuff_Burning_Own | Status_Debuff_Own | Status_Own;
        public static readonly TagBitmask32 Status_Debuff_Poisoned_Closure = Status_Debuff_Poisoned_Own | Status_Debuff_Own | Status_Own;

        /// <summary>
        /// 사용 예시:
        ///
        /// // 엔티티가 "Status.Debuff" 또는 그 자식 태그를 가지고 있는지 확인
        /// // (Stunned, Slowed, Burning, Poisoned 중 하나라도 있으면 true)
        /// bool hasDebuff = container.CombinedMask.ContainsAny(Status_Debuff_Closure);
        ///
        /// // 정확히 "Status.Debuff.Stunned" 태그를 가지고 있는지 확인
        /// bool isStunned = container.OwnTagsMask.ContainsAll(Status_Debuff_Stunned_Own);
        ///
        /// // 쿼리: "Status.Buff"가 있고 "Status.Debuff.Stunned"가 없어야 함
        /// var query = new TagQueryBitmask32
        /// {
        ///     RequiredMask = Status_Buff_Closure,
        ///     BlockedMask = Status_Debuff_Stunned_Own
        /// };
        /// bool matches = query.Matches(container.CombinedMask);
        /// </summary>
        public static void UsageExample() { }
    }
}
