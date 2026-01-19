using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class AttributeSystem : SystemBase
    {
        [BurstCompile]
        private partial struct AttributeJob : IJobEntity
        {
            public void Execute(ref AttributeData attributes)
            {
                // 속성 업데이트 로직 (예: 체력 회복 등)
            }
        }

        protected override void OnUpdate()
        {
            Dependency = new AttributeJob().ScheduleParallel(Dependency);
        }
    }
}
