using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GrantAbilityRequest를 소비해 DB 정의로부터 GrantedAbility를 생성하는 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityActivationGateSystem))]
    public class AbilityGrantSystem : SystemBase
    {
        private int _nextHandle;

        protected override void OnCreate()
        {
            base.OnCreate();
            _nextHandle = 1;
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            int nextHandle = _nextHandle;

            Entities.ForEach((DynamicBuffer<GrantAbilityRequest> requests, DynamicBuffer<GrantedAbility> granted) =>
            {
                for (int i = 0; i < requests.Length; i++)
                {
                    var req = requests[i];
                    if (!AbilityEffectDatabaseLookup.TryGetAbility(db, req.AbilityId, out var def))
                        continue;

                    var handle = new AbilityHandle { Value = nextHandle++, Version = 1 };

                    granted.Add(new GrantedAbility
                    {
                        Handle = handle,
                        AbilityId = def.AbilityId,
                        Level = req.Level,
                        Source = req.Source,
                        CooldownDuration = def.CooldownDuration,
                        CooldownRemaining = 0f,
                        ManaCost = def.ManaCost,
                        TagRequirements = def.TagRequirements,
                        CooldownEffectId = def.CooldownEffectId,
                        CooldownTag = def.CooldownTag,
                    });
                }

                requests.Clear();
            }).Schedule();

            _nextHandle = nextHandle;
        }
    }
}

