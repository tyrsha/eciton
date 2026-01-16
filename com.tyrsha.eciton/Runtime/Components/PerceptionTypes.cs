using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>진영/팀 식별자(스텁).</summary>
    public struct Faction : IComponentData
    {
        public int Value;
    }

    /// <summary>타겟으로 선택될 수 있는 엔티티 마커.</summary>
    public struct Targetable : IComponentData { }

    /// <summary>몬스터 인지(Perception) 센서 설정.</summary>
    public struct PerceptionSensor : IComponentData
    {
        public float Radius;
    }

    /// <summary>공격/스킬 사거리(타겟 인레인지 판정용).</summary>
    public struct AttackRange : IComponentData
    {
        public float Value;
    }
}

