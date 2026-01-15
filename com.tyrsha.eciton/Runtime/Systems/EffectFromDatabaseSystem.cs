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
                            EffectId = def.EffectId,
                            Level = req.Level,
                            Source = req.Source,
                            Target = req.Target,
                            Duration = def.Duration,
                            IsPermanent = def.IsPermanent,
                            IsPeriodic = def.IsPeriodic,
                            Period = def.Period,
                            GrantedTag = def.GrantedTag,
                            RevertModifierOnEnd = def.RevertModifierOnEnd,
                            StackingPolicy = def.StackingPolicy,
                            MaxStacks = def.MaxStacks,
                            Modifier = def.Modifier,
                        }
                    });
                }

                byId.Clear();
            }).Schedule();
        }
    }
}

