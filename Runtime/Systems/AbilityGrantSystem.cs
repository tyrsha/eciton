using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GrantAbilityRequest를 소비해 DB 정의로부터 GrantedAbility를 생성하는 시스템.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AbilityActivationGateSystem))]
    public partial struct AbilityGrantSystem : ISystem
    {
        private int _nextHandle;

        [BurstCompile]
        private partial struct AbilityGrantJob : IJobEntity
        {
            public AbilityEffectDatabase Db;
            public NativeReference<int> NextHandle;

            public void Execute(DynamicBuffer<GrantAbilityRequest> requests, DynamicBuffer<GrantedAbility> granted)
            {
                int nextHandle = NextHandle.Value;

                for (int i = 0; i < requests.Length; i++)
                {
                    var req = requests[i];
                    if (!AbilityEffectDatabaseLookup.TryGetAbility(Db, req.AbilityId, out var def))
                        continue;

                    var handle = new AbilityHandle { Value = nextHandle++, Version = 1 };

                    granted.Add(new GrantedAbility
                    {
                        Handle = handle,
                        AbilityId = def.AbilityId,
                        Level = req.Level,
                        Source = req.Source,
                        CooldownRemaining = 0f,
                    });
                }

                requests.Clear();
                NextHandle.Value = nextHandle;
            }
        }

        public void OnCreate(ref SystemState state)
        {
            _nextHandle = 1;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            using var nextHandleRef = new NativeReference<int>(AllocatorManager.TempJob);
            nextHandleRef.Value = _nextHandle;

            state.Dependency = new AbilityGrantJob
            {
                Db = db,
                NextHandle = nextHandleRef
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
            _nextHandle = nextHandleRef.Value;
        }
    }
}

