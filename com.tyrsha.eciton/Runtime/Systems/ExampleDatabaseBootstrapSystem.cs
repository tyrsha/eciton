using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 패키지 단독으로도 예제가 동작하도록 기본 DB를 생성하는 부트스트랩(스텁).
    /// 실제 게임 프로젝트에서는 Authoring/Baker로 대체하는 것을 권장.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ExampleDatabaseBootstrapSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            var em = EntityManager;
            using var q = em.CreateEntityQuery(typeof(AbilityEffectDatabase));
            if (q.CalculateEntityCount() > 0)
                return;

            // 기본 DB 생성
            var builder = new BlobBuilder(Allocator.Persistent);
            ref var root = ref builder.ConstructRoot<AbilityEffectDatabaseBlob>();

            var abilities = builder.Allocate(ref root.Abilities, 4);
            abilities[0] = new AbilityDefinition
            {
                AbilityId = CommonIds.Ability_Fireball,
                ExecutionType = AbilityExecutionType.SpawnProjectileAndApplyEffectsOnHit,
                CooldownDuration = 3f,
                ManaCost = 10f,
                ProjectileFlightTime = 0.35f,
                PrimaryEffectId = CommonIds.Effect_InstantDamage,
                SecondaryEffectId = CommonIds.Effect_BurnDot,
                CleanseTag = GameplayTag.Invalid,
                TagRequirements = default
            };
            abilities[1] = new AbilityDefinition
            {
                AbilityId = CommonIds.Ability_Heal,
                ExecutionType = AbilityExecutionType.ApplyEffectToTarget,
                CooldownDuration = 2f,
                ManaCost = 8f,
                PrimaryEffectId = CommonIds.Effect_HealInstant,
                SecondaryEffectId = 0,
                CleanseTag = GameplayTag.Invalid,
                TagRequirements = default
            };
            abilities[2] = new AbilityDefinition
            {
                AbilityId = CommonIds.Ability_Cleanse,
                ExecutionType = AbilityExecutionType.CleanseByTag,
                CooldownDuration = 5f,
                ManaCost = 5f,
                PrimaryEffectId = 0,
                SecondaryEffectId = 0,
                CleanseTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                TagRequirements = default
            };
            abilities[3] = new AbilityDefinition
            {
                AbilityId = CommonIds.Ability_StunBolt,
                ExecutionType = AbilityExecutionType.SpawnProjectileAndApplyEffectsOnHit,
                CooldownDuration = 4f,
                ManaCost = 12f,
                ProjectileFlightTime = 0.25f,
                PrimaryEffectId = CommonIds.Effect_Stun,
                SecondaryEffectId = 0,
                CleanseTag = GameplayTag.Invalid,
                TagRequirements = default
            };

            var effects = builder.Allocate(ref root.Effects, 5);
            effects[0] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_InstantDamage,
                Duration = 0f,
                IsPermanent = true,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = GameplayTag.Invalid,
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.None,
                MaxStacks = 1,
            };
            var e0Mods = builder.Allocate(ref effects[0].Modifiers, 1);
            e0Mods[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = -30f };
            effects[1] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_BurnDot,
                Duration = 5f,
                IsPermanent = false,
                IsPeriodic = true,
                Period = 1f,
                GrantedTag = new GameplayTag { Value = CommonIds.Tag_Burning },
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.RefreshDuration,
                MaxStacks = 1,
            };
            var e1Mods = builder.Allocate(ref effects[1].Modifiers, 1);
            e1Mods[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = -4f };
            effects[2] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_HealInstant,
                Duration = 0f,
                IsPermanent = true,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = GameplayTag.Invalid,
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.None,
                MaxStacks = 1,
            };
            var e2Mods = builder.Allocate(ref effects[2].Modifiers, 1);
            e2Mods[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = 25f };
            effects[3] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_Stun,
                Duration = 2f,
                IsPermanent = false,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = new GameplayTag { Value = CommonIds.Tag_Stunned },
                RevertModifierOnEnd = false,
                StackingPolicy = EffectStackingPolicy.RefreshDuration,
                MaxStacks = 1,
            };
            var e3Mods = builder.Allocate(ref effects[3].Modifiers, 1);
            e3Mods[0] = new AttributeModifier { Attribute = AttributeId.Health, Op = AttributeModOp.Add, Magnitude = 0f };
            effects[4] = new EffectDefinition
            {
                EffectId = CommonIds.Effect_Slow,
                Duration = 2f,
                IsPermanent = false,
                IsPeriodic = false,
                Period = 0f,
                GrantedTag = new GameplayTag { Value = CommonIds.Tag_Slowed },
                RevertModifierOnEnd = true,
                StackingPolicy = EffectStackingPolicy.RefreshDuration,
                MaxStacks = 1,
            };
            var e4Mods = builder.Allocate(ref effects[4].Modifiers, 1);
            e4Mods[0] = new AttributeModifier { Attribute = AttributeId.MoveSpeed, Op = AttributeModOp.Multiply, Magnitude = 0.5f };

            var blobRef = builder.CreateBlobAssetReference<AbilityEffectDatabaseBlob>(Allocator.Persistent);
            builder.Dispose();

            var dbEntity = em.CreateEntity();
            em.AddComponentData(dbEntity, new AbilityEffectDatabase { Blob = blobRef });
        }

        protected override void OnUpdate() { }
    }
}

