using Unity.Burst;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>Threat 테이블을 시간에 따라 감쇠시키는 스텁.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ThreatSystem))]
    public partial struct ThreatDecaySystem : ISystem
    {
        [BurstCompile]
        private partial struct ThreatDecayJob : IJobEntity
        {
            public float Dt;
            public double Now;

            public void Execute(in PerceptionSensor sensor, DynamicBuffer<ThreatEntry> threat)
            {
                float decay = sensor.ThreatDecayPerSecond;
                float mem = sensor.MemorySeconds;

                for (int i = threat.Length - 1; i >= 0; i--)
                {
                    var e = threat[i];

                    if (decay > 0f && e.Threat > 0f)
                    {
                        e.Threat -= decay * Dt;
                        if (e.Threat < 0f) e.Threat = 0f;
                    }

                    // 메모리 만료면 제거(Threat가 0이고 오래 못 봤으면 제거)
                    if (mem > 0f && e.LastSeenTime > 0 && (Now - e.LastSeenTime) > mem && e.Threat <= 0f)
                    {
                        threat.RemoveAt(i);
                        continue;
                    }

                    threat[i] = e;
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ThreatDecayJob
            {
                Dt = SystemAPI.Time.DeltaTime,
                Now = SystemAPI.Time.ElapsedTime
            }.ScheduleParallel(state.Dependency);
        }
    }
}

