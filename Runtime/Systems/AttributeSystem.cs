using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AttributeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AttributeData attributes) =>
            {
                // 속성 업데이트 로직 (예: 체력 회복 등)
            }).ScheduleParallel();
        }
    }
}