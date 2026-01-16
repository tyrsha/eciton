using Tyrsha.Eciton;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// DOTS Perception/Combat 세팅을 베이크하는 Authoring.
    /// - isTargetable=true면 Targetable을 추가(다른 AI가 나를 타겟으로 삼을 수 있음)
    /// - isSensor=true면 PerceptionSensor/AttackRange를 추가(내가 타겟을 찾음)
    /// </summary>
    public class PerceptionAuthoring : MonoBehaviour
    {
        public int Faction = 1;
        public bool IsTargetable = true;
        public bool IsSensor = true;
        public float SensorRadius = 10f;
        public float AttackRange = 2f;
    }

    public class PerceptionBaker : Baker<PerceptionAuthoring>
    {
        public override void Bake(PerceptionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Tyrsha.Eciton.Faction { Value = authoring.Faction });

            if (authoring.IsTargetable)
                AddComponent(entity, new Targetable());

            if (authoring.IsSensor)
            {
                AddComponent(entity, new PerceptionSensor { Radius = authoring.SensorRadius });
                AddComponent(entity, new AttackRange { Value = authoring.AttackRange });
            }

            // 위치 기반 인지를 위해 LocalTransform을 사용(TransformUsageFlags.Dynamic로 자동 포함됨)
        }
    }
}

