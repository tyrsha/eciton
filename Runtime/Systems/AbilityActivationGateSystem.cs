using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// Ability 활성화 요청을 실행하기 전에 공통 게이트(태그/쿨다운/코스트)를 적용하는 스텁 시스템.
    /// 통과한 요청만 다음 Ability 시스템들이 소비한다.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FireballAbilitySystem))]
    [UpdateBefore(typeof(CommonAbilitySystems))]
    public partial struct AbilityActivationGateSystem : ISystem
    {
        [BurstCompile]
        private partial struct AbilityActivationGateJob : IJobEntity
        {
            public AbilityEffectDatabase Db;
            public float Dt;

            public void Execute(
                Entity e,
                ref AttributeData attributes,
                in DynamicBuffer<GameplayTagElement> tags,
                DynamicBuffer<GrantedAbility> granted,
                DynamicBuffer<TryActivateAbilityRequest> tryActivate,
                DynamicBuffer<ApplyEffectByIdRequest> applyEffectById)
            {
                // 쿨다운 감소
                for (int g = 0; g < granted.Length; g++)
                {
                    var ga = granted[g];
                    if (ga.CooldownRemaining > 0f)
                    {
                        ga.CooldownRemaining -= Dt;
                        if (ga.CooldownRemaining < 0f) ga.CooldownRemaining = 0f;
                        granted[g] = ga;
                    }
                }

                // 요청 검사
                for (int i = tryActivate.Length - 1; i >= 0; i--)
                {
                    var req = tryActivate[i];

                    int idx = -1;
                    for (int g = 0; g < granted.Length; g++)
                    {
                        if (granted[g].Handle.Value == req.Handle.Value)
                        {
                            idx = g;
                            break;
                        }
                    }

                    if (idx < 0)
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    var ability = granted[idx];
                    if (!AbilityEffectDatabaseLookup.TryGetAbility(Db, ability.AbilityId, out var def))
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    // 쿨다운 태그 방식: 태그가 있으면 실패
                    if (def.CooldownTag.IsValid && HasTag(tags, def.CooldownTag.Value))
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    // 태그 요구조건(단일 Required/Blocked 스텁)
                    if (def.TagRequirements.Required.IsValid && !HasTag(tags, def.TagRequirements.Required.Value))
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }
                    if (def.TagRequirements.Blocked.IsValid && HasTag(tags, def.TagRequirements.Blocked.Value))
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    // 쿨다운
                    if (ability.CooldownRemaining > 0f)
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    // 마나 코스트
                    if (def.ManaCost > 0f && attributes.Mana < def.ManaCost)
                    {
                        tryActivate.RemoveAt(i);
                        continue;
                    }

                    // 통과: 코스트 지불 + 쿨다운 시작 (스텁: 성공 시 바로 차감)
                    if (def.ManaCost > 0f)
                        attributes.Mana -= def.ManaCost;
                    if (def.CooldownEffectId == 0 && def.CooldownDuration > 0f)
                    {
                        ability.CooldownRemaining = def.CooldownDuration;
                        granted[idx] = ability;
                    }

                    // 쿨다운을 Effect로 표현(권장): 성공 시 self에 쿨다운 effect 적용
                    if (def.CooldownEffectId != 0)
                    {
                        applyEffectById.Add(new ApplyEffectByIdRequest
                        {
                            EffectId = def.CooldownEffectId,
                            Level = 1,
                            Source = e,
                            Target = e
                        });
                    }
                }
            }

            private static bool HasTag(in DynamicBuffer<GameplayTagElement> tags, int tagValue)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i].Tag.Value == tagValue)
                        return true;
                }
                return false;
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            state.Dependency = new AbilityActivationGateJob
            {
                Db = db,
                Dt = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
    }
}

