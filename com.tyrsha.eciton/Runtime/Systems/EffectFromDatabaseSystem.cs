using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// ApplyEffectByIdRequest를 DB(Blob) 정의로 EffectSpec으로 변환해 ApplyEffectRequest로 전달하는 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EffectRequestSystem))]
    public class EffectFromDatabaseSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            Entities.ForEach((
                DynamicBuffer<ApplyEffectByIdRequest> byId,
                DynamicBuffer<ApplyEffectRequest> apply) =>
            {
                for (int i = 0; i < byId.Length; i++)
                {
                    var req = byId[i];
                    if (!AbilityEffectDatabaseLookup.TryGetEffect(db, req.EffectId, out var def))
                        continue;

                    apply.Add(new ApplyEffectRequest
                    {
                        Spec = new EffectSpec
                        {
                            // 정의 값은 EffectRequestSystem이 Blob DB에서 조회한다.
                            EffectId = def.EffectId,
                            Level = req.Level,
                            Source = req.Source,
                            Target = req.Target,
                            Duration = 0f,
                            IsPermanent = false,
                            IsPeriodic = false,
                            Period = 0f,
                            GrantedTag = GameplayTag.Invalid,
                            BlockedByTag = GameplayTag.Invalid,
                            RevertModifierOnEnd = false,
                            StackingPolicy = EffectStackingPolicy.None,
                            MaxStacks = 0,
                            Modifier = default,
                            Modifiers = default,
                        }
                    });
                }

                byId.Clear();
            }).Schedule();
        }
    }
}

