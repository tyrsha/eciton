using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>Threat 테이블을 시간에 따라 감쇠시키는 스텁.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ThreatSystem))]
    public class ThreatDecaySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            double now = SystemAPI.Time.ElapsedTime;

            Entities.ForEach((in PerceptionSensor sensor, DynamicBuffer<ThreatEntry> threat) =>
            {
                float decay = sensor.ThreatDecayPerSecond;
                float mem = sensor.MemorySeconds;

                for (int i = threat.Length - 1; i >= 0; i--)
                {
                    var e = threat[i];

                    if (decay > 0f && e.Threat > 0f)
                    {
                        e.Threat -= decay * dt;
                        if (e.Threat < 0f) e.Threat = 0f;
                    }

                    // 메모리 만료면 제거(Threat가 0이고 오래 못 봤으면 제거)
                    if (mem > 0f && e.LastSeenTime > 0 && (now - e.LastSeenTime) > mem && e.Threat <= 0f)
                    {
                        threat.RemoveAt(i);
                        continue;
                    }

                    threat[i] = e;
                }
            }).ScheduleParallel();
        }
    }
}

