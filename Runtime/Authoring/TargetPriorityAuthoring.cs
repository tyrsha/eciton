using Tyrsha.Eciton;
using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>타겟 우선순위(스텁) Authoring.</summary>
    public class TargetPriorityAuthoring : MonoBehaviour
    {
        public float Weight = 0f;
    }

    public class TargetPriorityBaker : Baker<TargetPriorityAuthoring>
    {
        public override void Bake(TargetPriorityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TargetPriority { Weight = authoring.Weight });
        }
    }
}

