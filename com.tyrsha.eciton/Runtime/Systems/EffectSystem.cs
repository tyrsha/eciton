using Unity.Entities;

namespace Tyrsha.Eciton
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class EffectSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref EffectBase effect, ref AttributeData attribute) =>
            {
                // 효과 적용 로직 (예: Attribute 값 변경)
                if (!effect.IsPermanent)
                {
                    effect.Duration -= Time.DeltaTime;
                }
            }).ScheduleParallel();
        }
    }
}