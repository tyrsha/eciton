using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// ApplyAttributeModifierRequest를 AttributeData에 반영하는 최소 스텁 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ActiveEffectSystem))]
    public class AttributeModifierSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // 저항 컴포넌트가 있는 경우
            Entities.ForEach((Entity e, ref AttributeData attributes, ref DamageResistanceData resist, DynamicBuffer<ApplyAttributeModifierRequest> requests, DynamicBuffer<PendingGameplayEvent> events) =>
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    var mod = requests[i].Modifier;
                    ApplyModifier(ref attributes, ref resist, mod);
                    EmitEvent(e, events, mod);
                }

                requests.Clear();
            }).ScheduleParallel();

            // 저항 컴포넌트가 없는 경우
            Entities.WithNone<DamageResistanceData>().ForEach((Entity e, ref AttributeData attributes, DynamicBuffer<ApplyAttributeModifierRequest> requests, DynamicBuffer<PendingGameplayEvent> events) =>
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    var mod = requests[i].Modifier;
                    ApplyModifier(ref attributes, mod);
                    EmitEvent(e, events, mod);
                }

                requests.Clear();
            }).ScheduleParallel();
        }

        private static void ApplyModifier(ref AttributeData data, ref DamageResistanceData resist, AttributeModifier mod)
        {
            // NOTE: 스텁 수준에서는 AttributeData의 4개 필드만 지원.
            // 이후 확장에서는 AttributeSet/Blob/런타임 레지스트리로 일반화.
            switch (mod.Attribute)
            {
                case AttributeId.Health:
                    // 스텁: Health에 들어오는 음수 Add는 "데미지"로 간주하고 Shield가 있으면 먼저 흡수한다.
                    if (mod.Op == AttributeModOp.Add && mod.Magnitude < 0f)
                    {
                        float damage = ApplyResistance(-mod.Magnitude, mod.DamageType, resist);
                        if (data.Shield > 0f)
                        {
                            float absorbed = data.Shield < damage ? data.Shield : damage;
                            data.Shield -= absorbed;
                            damage -= absorbed;
                        }

                        if (damage > 0f)
                            data.Health -= damage;
                    }
                    else
                    {
                        data.Health = ApplyOp(data.Health, mod.Op, mod.Magnitude);
                    }
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
                case AttributeId.Shield:
                    data.Shield = ApplyOp(data.Shield, mod.Op, mod.Magnitude);
                    break;
                case AttributeId.MoveSpeed:
                    data.MoveSpeed = ApplyOp(data.MoveSpeed, mod.Op, mod.Magnitude);
                    break;
            }
        }

        private static float ApplyResistance(float rawDamageMagnitude, DamageType type, DamageResistanceData resist)
        {
            // rawDamageMagnitude는 음수(Add)에서 -Magnitude로 들어온 양수값
            float resistance = 0f;
            switch (type)
            {
                case DamageType.Fire:
                    resistance = resist.FireResistance;
                    break;
                case DamageType.Ice:
                    resistance = resist.IceResistance;
                    break;
                case DamageType.Poison:
                    resistance = resist.PoisonResistance;
                    break;
                case DamageType.Physical:
                    resistance = resist.PhysicalResistance;
                    break;
                default:
                    resistance = 0f;
                    break;
            }

            if (resistance <= 0f) return rawDamageMagnitude;
            if (resistance >= 1f) return 0f;
            return rawDamageMagnitude * (1f - resistance);
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

        private static void ApplyModifier(ref AttributeData data, AttributeModifier mod)
        {
            // 저항 데이터가 없으면 기존 동작
            switch (mod.Attribute)
            {
                case AttributeId.Health:
                    if (mod.Op == AttributeModOp.Add && mod.Magnitude < 0f)
                    {
                        float damage = -mod.Magnitude;
                        if (data.Shield > 0f)
                        {
                            float absorbed = data.Shield < damage ? data.Shield : damage;
                            data.Shield -= absorbed;
                            damage -= absorbed;
                        }
                        if (damage > 0f)
                            data.Health -= damage;
                    }
                    else
                    {
                        data.Health = ApplyOp(data.Health, mod.Op, mod.Magnitude);
                    }
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
                case AttributeId.Shield:
                    data.Shield = ApplyOp(data.Shield, mod.Op, mod.Magnitude);
                    break;
                case AttributeId.MoveSpeed:
                    data.MoveSpeed = ApplyOp(data.MoveSpeed, mod.Op, mod.Magnitude);
                    break;
            }
        }

        private static void EmitEvent(Entity entity, DynamicBuffer<PendingGameplayEvent> events, AttributeModifier mod)
        {
            if (mod.Attribute != AttributeId.Health || mod.Op != AttributeModOp.Add || mod.Magnitude == 0f)
                return;

            if (mod.Magnitude < 0f)
            {
                events.Add(new PendingGameplayEvent
                {
                    Event = new GameplayEvent
                    {
                        Type = GameplayEventType.DamageApplied,
                        Source = Entity.Null,
                        Target = entity,
                        Id = (int)mod.DamageType,
                        Magnitude = -mod.Magnitude
                    }
                });
            }
            else
            {
                events.Add(new PendingGameplayEvent
                {
                    Event = new GameplayEvent
                    {
                        Type = GameplayEventType.HealApplied,
                        Source = Entity.Null,
                        Target = entity,
                        Id = 0,
                        Magnitude = mod.Magnitude
                    }
                });
            }
        }
    }
}

