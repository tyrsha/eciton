using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// ExecutionType=SpawnProjectileAndApplyEffectsOnHit 용 범용 투사체 스텁.
    /// </summary>
    public struct AbilityProjectile : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float RemainingFlightTime;

        public int PrimaryEffectId;
        public int SecondaryEffectId;
    }
}

