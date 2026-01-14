using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 런타임에서 Ability/Effect 정의를 데이터로 조회하기 위한 DB(Blob) 스텁.
    /// Authoring(Baker) 또는 테스트/예제 부트스트랩이 이 DB를 생성한다.
    /// </summary>
    public struct AbilityEffectDatabase : IComponentData
    {
        public BlobAssetReference<AbilityEffectDatabaseBlob> Blob;
    }

    public enum AbilityExecutionType : byte
    {
        None = 0,
        ApplyEffectToTarget = 1,
        SpawnProjectileAndApplyEffectsOnHit = 2,
        CleanseByTag = 3,
    }

    public struct AbilityDefinition
    {
        public int AbilityId;
        public AbilityExecutionType ExecutionType;

        public float CooldownDuration;
        public float ManaCost;
        public TagQuery TagRequirements;

        // Execution payload (스텁)
        public float ProjectileFlightTime;
        public int PrimaryEffectId;
        public int SecondaryEffectId;
        public GameplayTag CleanseTag;

        // Data-driven gate via effects/tags (optional)
        public int CooldownEffectId;
        public GameplayTag CooldownTag;
    }

    public struct EffectDefinition
    {
        public int EffectId;

        public float Duration;
        public bool IsPermanent;

        public bool IsPeriodic;
        public float Period;

        public GameplayTag GrantedTag;
        public bool RevertModifierOnEnd;

        public EffectStackingPolicy StackingPolicy;
        public int MaxStacks;

        /// <summary>이 태그를 대상이 가지고 있으면 적용을 막는다(면역/저항 스텁).</summary>
        public GameplayTag BlockedByTag;

        public BlobArray<AttributeModifier> Modifiers;
    }

    public struct AbilityEffectDatabaseBlob
    {
        public BlobArray<AbilityDefinition> Abilities;
        public BlobArray<EffectDefinition> Effects;
    }

    public static class AbilityEffectDatabaseLookup
    {
        public static bool TryGetAbility(in AbilityEffectDatabase db, int abilityId, out AbilityDefinition def)
        {
            if (!db.Blob.IsCreated)
            {
                def = default;
                return false;
            }

            ref var arr = ref db.Blob.Value.Abilities;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].AbilityId == abilityId)
                {
                    def = arr[i];
                    return true;
                }
            }

            def = default;
            return false;
        }

        public static bool TryGetEffect(in AbilityEffectDatabase db, int effectId, ref EffectDefinition def)
        {
            if (!db.Blob.IsCreated)
            {
                def = default;
                return false;
            }

            ref var arr = ref db.Blob.Value.Effects;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].EffectId == effectId)
                {
                    def = arr[i];
                    return true;
                }
            }

            def = default;
            return false;
        }
    }
}

