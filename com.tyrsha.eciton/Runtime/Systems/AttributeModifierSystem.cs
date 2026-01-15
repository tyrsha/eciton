using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// ApplyAttributeModifierRequest를 AttributeData에 반영하는 최소 스텁 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AttributeModifierSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref AttributeData attributes, DynamicBuffer<ApplyAttributeModifierRequest> requests) =>
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    var mod = requests[i].Modifier;
                    ApplyModifier(ref attributes, mod);
                }

                requests.Clear();
            }).ScheduleParallel();
        }

        private static void ApplyModifier(ref AttributeData data, AttributeModifier mod)
        {
            // NOTE: 스텁 수준에서는 AttributeData의 4개 필드만 지원.
            // 이후 확장에서는 AttributeSet/Blob/런타임 레지스트리로 일반화.
            switch (mod.Attribute)
            {
                case AttributeId.Health:
                    data.Health = ApplyOp(data.Health, mod.Op, mod.Magnitude);
                    break;
                case AttributeId.Mana:
                    data.Mana = ApplyOp(data.Mana, mod.Op, mod.Magnitude);
                    break;
                case AttributeId.Strength:
                    data.Strength = ApplyOp(data.Strength, mod.Op, mod.Magnitude);
                    break;
                case AttributeId.Agility:
                    data.Agility = ApplyOp(data.Agility, mod.Op, mod.Magnitude);
                    break;
            }
        }

        private static float ApplyOp(float current, AttributeModOp op, float magnitude)
        {
            switch (op)
            {
                case AttributeModOp.Add:
                    return current + magnitude;
                case AttributeModOp.Multiply:
                    return current * magnitude;
                case AttributeModOp.Override:
                    return magnitude;
                default:
                    return current;
            }
        }
    }
}

