using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>예제 시나리오용 스턴 볼트 투사체 스텁.</summary>
    public struct StunBoltProjectile : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float RemainingFlightTime;
        public int EffectId;
    }
}

