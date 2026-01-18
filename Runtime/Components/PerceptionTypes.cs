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

    /// <summary>
    /// 타겟 우선순위(스텁). 값이 클수록 선호.
    /// 예: 힐러/딜러 우선 타겟팅, 보스/소환수 우선 등.
    /// </summary>
    public struct TargetPriority : IComponentData
    {
        public float Weight;
    }

    /// <summary>몬스터 인지(Perception) 센서 설정.</summary>
    public struct PerceptionSensor : IComponentData
    {
        public float Radius;

        /// <summary>메모리(최근 타겟 유지) 시간(초). 0이면 즉시 잊음.</summary>
        public float MemorySeconds;

        /// <summary>위협도(Threat) 감쇠량(초당).</summary>
        public float ThreatDecayPerSecond;

        /// <summary>거리 패널티 가중치(Score에서 distSq * weight 만큼 감점).</summary>
        public float DistanceWeight;

        /// <summary>타겟 스위치 히스테리시스. 현재 타겟보다 이 값만큼 좋아야 바꾼다.</summary>
        public float SwitchHysteresis;

        /// <summary>LOS(시야/가시성) 훅을 사용하도록 강제.</summary>
        public byte RequireLineOfSight;
    }

    /// <summary>공격/스킬 사거리(타겟 인레인지 판정용).</summary>
    public struct AttackRange : IComponentData
    {
        public float Value;
    }

    /// <summary>
    /// LOS 훅: 외부(물리/레이캐스트) 시스템이 채워주는 "현재 보이는 타겟" 목록.
    /// RequireLineOfSight=1 일 때 이 버퍼가 있으면, 이 안에 있는 타겟만 후보로 취급한다.
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct VisibleTarget : IBufferElementData
    {
        public Entity Target;
    }

    /// <summary>Threat 테이블(타겟별 위협도) 엔트리.</summary>
    [InternalBufferCapacity(8)]
    public struct ThreatEntry : IBufferElementData
    {
        public Entity Target;
        public float Threat;
        public double LastSeenTime;
        public double LastAggroTime;
    }
}

