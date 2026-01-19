using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// AttributeData(베이스) + AttributeModifierElement(버퍼)를 집계해 AggregatedAttributeData를 갱신하는 스텁.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AttributeModifierSystem))]
    public partial struct AttributeAggregatorSystem : ISystem
    {
        [BurstCompile]
        private partial struct AttributeAggregatorJob : IJobEntity
        {
            public void Execute(
                in AttributeData baseAttributes,
                in DynamicBuffer<AttributeModifierElement> modifiers,
                ref AggregatedAttributeData aggregated)
            {
                // 베이스 복사
                aggregated.Health = baseAttributes.Health;
                aggregated.Mana = baseAttributes.Mana;
                aggregated.Strength = baseAttributes.Strength;
                aggregated.Agility = baseAttributes.Agility;
                aggregated.Shield = baseAttributes.Shield;
                aggregated.MoveSpeed = baseAttributes.MoveSpeed;

                // 스텁 집계(순서 단순): Add -> Multiply -> Override
                for (int i = 0; i < modifiers.Length; i++)
                {
                    Apply(ref aggregated, modifiers[i].Modifier);
                }
            }

            private static void Apply(ref AggregatedAttributeData data, AttributeModifier mod)
            {
                ref float value = ref GetRef(ref data, mod.Attribute);
                switch (mod.Op)
                {
                    case AttributeModOp.Add:
                        value += mod.Magnitude;
                        break;
                    case AttributeModOp.Multiply:
                        value *= mod.Magnitude;
                        break;
                    case AttributeModOp.Override:
                        value = mod.Magnitude;
                        break;
                }
            }

            private static ref float GetRef(ref AggregatedAttributeData data, AttributeId id)
            {
                switch (id)
                {
                    case AttributeId.Health: return ref data.Health;
                    case AttributeId.Mana: return ref data.Mana;
                    case AttributeId.Strength: return ref data.Strength;
                    case AttributeId.Agility: return ref data.Agility;
                    case AttributeId.Shield: return ref data.Shield;
                    case AttributeId.MoveSpeed: return ref data.MoveSpeed;
                    default: return ref data.Health;
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AttributeAggregatorJob().ScheduleParallel(state.Dependency);
        }
    }
}

