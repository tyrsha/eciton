using Unity.Entities;

namespace Tyrsha.Eciton
{
    public enum DamageType : byte
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Poison = 3,
        Physical = 4,
    }

    /// <summary>
    /// (스텁) 데미지 저항/면역 정보를 저장하는 컴포넌트.
    /// 저항은 0~1 범위를 가정(예: 0.2면 20% 감소).
    /// </summary>
    public struct DamageResistanceData : IComponentData
    {
        public float FireResistance;
        public float IceResistance;
        public float PoisonResistance;
        public float PhysicalResistance;
    }
}

