using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// Ability/Effect 정의 에셋들을 모아 런타임 DB(Blob)로 베이크하는 Authoring 컴포넌트.
    /// </summary>
    public class AbilityEffectDatabaseAuthoring : MonoBehaviour
    {
        public AbilityEffectDatabaseAsset DatabaseAsset;
        public AbilityDefinitionAsset[] Abilities;
        public EffectDefinitionAsset[] Effects;
    }

    public class AbilityEffectDatabaseBaker : Baker<AbilityEffectDatabaseAuthoring>
    {
        public override void Bake(AbilityEffectDatabaseAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var abilitiesSrc = authoring.DatabaseAsset != null ? authoring.DatabaseAsset.Abilities : authoring.Abilities;
            var effectsSrc = authoring.DatabaseAsset != null ? authoring.DatabaseAsset.Effects : authoring.Effects;

            using var builder = new BlobBuilder(AllocatorManager.Persistent);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, abilitiesSrc?.Length ?? 0);
            for (int i = 0; i < abilities.Length; i++)
            {
                var a = abilitiesSrc[i];
                abilities[i] = new AbilityDefinition
                {
                    AbilityId = a.AbilityId,
                    ExecutionType = a.ExecutionType,
                    CooldownDuration = a.CooldownDuration,
                    ManaCost = a.ManaCost,
                    ProjectileFlightTime = a.ProjectileFlightTime,
                    PrimaryEffectId = a.PrimaryEffectId,
                    SecondaryEffectId = a.SecondaryEffectId,
                    CleanseTag = a.CleanseTagValue == 0 ? GameplayTag.Invalid : new GameplayTag { Value = a.CleanseTagValue },
                    TagRequirements = default,
                    CooldownEffectId = a.CooldownEffectId,
                    CooldownTag = a.CooldownTagValue == 0 ? GameplayTag.Invalid : new GameplayTag { Value = a.CooldownTagValue },
                };
            }

            var effects = builder.Allocate(ref root.Effects, effectsSrc?.Length ?? 0);
            for (int i = 0; i < effects.Length; i++)
            {
                var e = effectsSrc[i];
                effects[i] = new EffectDefinition
                {
                    EffectId = e.EffectId,
                    Duration = e.Duration,
                    IsPermanent = e.IsPermanent,
                    IsPeriodic = e.IsPeriodic,
                    Period = e.Period,
                    GrantedTag = e.GrantedTagValue == 0 ? GameplayTag.Invalid : new GameplayTag { Value = e.GrantedTagValue },
                    RevertModifierOnEnd = e.RevertModifierOnEnd,
                    StackingPolicy = e.StackingPolicy,
                    MaxStacks = e.MaxStacks <= 0 ? 1 : e.MaxStacks,
                    BlockedByTag = e.BlockedByTagValue == 0 ? GameplayTag.Invalid : new GameplayTag { Value = e.BlockedByTagValue },
                };

                // Modifiers allocate per effect
                int modCount = (e.Modifiers != null && e.Modifiers.Length > 0) ? e.Modifiers.Length : 1;
                var mods = builder.Allocate(ref effects[i].Modifiers, modCount);
                if (e.Modifiers != null && e.Modifiers.Length > 0)
                {
                    for (int m = 0; m < e.Modifiers.Length; m++)
                    {
                        var src = e.Modifiers[m];
                        mods[m] = new AttributeModifier
                        {
                            Attribute = src.Attribute,
                            Op = src.Op,
                            Magnitude = src.Magnitude,
                            DamageType = src.DamageType
                        };
                    }
                }
                else
                {
                    mods[0] = new AttributeModifier
                    {
                        Attribute = e.ModifierAttribute,
                        Op = e.ModifierOp,
                        Magnitude = e.ModifierMagnitude,
                        DamageType = e.ModifierDamageType
                    };
                }
            }

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(AllocatorManager.Persistent);
            AddComponent(entity, new AbilityEffectDatabase { Blob = blob });
        }
    }
}

