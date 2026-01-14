using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AbilitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AbilityBase ability, in EffectBase effect) =>
            {
                if (ability.IsActive)
                {
                    // 능력을 실행하고 효과를 적용
                }
            }).ScheduleParallel();
        }
    }
}