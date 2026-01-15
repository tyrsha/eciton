using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 예제 시나리오용 Fireball 투사체 스텁.
    /// 실제 이동/충돌은 게임 프로젝트에서 물리/트랜스폼과 연동해 확장한다.
    /// </summary>
    public struct FireballProjectile : IComponentData
    {
        public Entity Source;
        public Entity Target;

        /// <summary>충돌까지 남은 비행 시간(초). 0 이하가 되면 충돌/폭발 처리.</summary>
        public float RemainingFlightTime;

        public float ImpactDamage;

        public float BurnDuration;
        public float BurnDamagePerSecond;
        public float BurnTickPeriod;
    }

    /// <summary>예제 전용 Ability/Effect ID 모음.</summary>
    public static class ExampleIds
    {
        public const int Ability_Fireball = 1;

        public const int Effect_FireballImpactDamage = 1;
        public const int Effect_BurnDot = 2;
    }
}

