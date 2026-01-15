using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// Ability/Effect 정의 에셋들을 모아 런타임 DB(Blob)로 베이크하는 Authoring 컴포넌트.
    /// </summary>
    public class AbilityEffectDatabaseAuthoring : MonoBehaviour
    {
        public AbilityDefinitionAsset[] Abilities;
        public EffectDefinitionAsset[] Effects;
    }

    public class AbilityEffectDatabaseBaker : Baker<AbilityEffectDatabaseAuthoring>
    {
        public override void Bake(AbilityEffectDatabaseAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            using var builder = new BlobBuilder(Allocator.Persistent);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, authoring.Abilities?.Length ?? 0);
            for (int i = 0; i < abilities.Length; i++)
            {
                var a = authoring.Abilities[i];
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
                    TagRequirements = default
                };
            }

            var effects = builder.Allocate(ref root.Effects, authoring.Effects?.Length ?? 0);
            for (int i = 0; i < effects.Length; i++)
            {
                var e = authoring.Effects[i];
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
                    Modifier = new AttributeModifier
                    {
                        Attribute = e.ModifierAttribute,
                        Op = e.ModifierOp,
                        Magnitude = e.ModifierMagnitude
                    }
                };
            }

            var blob = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(Allocator.Persistent);
            AddComponent(entity, new AbilityEffectDatabase { Blob = blob });
        }
    }
}

