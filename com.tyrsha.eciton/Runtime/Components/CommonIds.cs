namespace Tyrsha.Eciton
{
    /// <summary>
    /// 예제/스텁용 공통 ID 모음.
    /// 프로젝트에서는 별도 DB/해시/테이블로 대체하는 것을 권장.
    /// </summary>
    public static class CommonIds
    {
        // Gameplay Tags
        public const int Tag_Burning = 1001;
        public const int Tag_Stunned = 1002;
        public const int Tag_Slowed = 1003;

        // Abilities
        public const int Ability_Fireball = 1;
        public const int Ability_Heal = 2;
        public const int Ability_Cleanse = 3;
        public const int Ability_StunBolt = 4;

        // Effects
        public const int Effect_InstantDamage = 1;
        public const int Effect_BurnDot = 2;
        public const int Effect_HealInstant = 3;
        public const int Effect_Stun = 4;
        public const int Effect_Slow = 5;
        public const int Effect_Shield = 6;
        public const int Effect_RegenHot = 7;
    }
}

