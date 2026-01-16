using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GameplayEventQueue를 기반으로 Threat(aggro)를 누적/갱신하는 스텁.
    /// - EffectApplied 이벤트에서, 해당 Effect 정의가 "데미지(Health Add 음수)"라면 Threat에 반영한다.
    /// - 실제 게임에서는 힐/버프/도발/거리/시야 등 다양한 규칙을 확장.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayEventDispatchSystem))]
    public class ThreatSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonEntity<GameplayEventQueueSingleton>(out var queueEntity))
                return;
            if (!SystemAPI.TryGetSingleton<AbilityEffectDatabase>(out var db))
                return;

            var em = EntityManager;
            var queue = em.GetBuffer<GameplayEventQueue>(queueEntity);

            double now = SystemAPI.Time.ElapsedTime;

            for (int i = 0; i < queue.Length; i++)
            {
                var evt = queue[i].Event;
                if (evt.Type != GameplayEventType.EffectApplied)
                    continue;
                if (evt.Source == Entity.Null || evt.Target == Entity.Null)
                    continue;
                if (!em.Exists(evt.Target) || !em.Exists(evt.Source))
                    continue;
                if (!em.HasBuffer<ThreatEntry>(evt.Target))
                    continue; // 타겟이 센서/AI가 아니면 스킵

                // Effect 정의를 보고 데미지량 합산(Health Add 음수)
                if (!AbilityEffectDatabaseLookup.TryGetEffect(db, evt.Id, out var def))
                    continue;

                float damage = 0f;
                for (int m = 0; m < def.Modifiers.Length; m++)
                {
                    var mod = def.Modifiers[m];
                    if (mod.Attribute == AttributeId.Health && mod.Op == AttributeModOp.Add && mod.Magnitude < 0f)
                        damage += -mod.Magnitude;
                }
                if (damage <= 0f)
                    continue;

                var threatBuf = em.GetBuffer<ThreatEntry>(evt.Target);
                AddThreat(threatBuf, evt.Source, damage, now);
            }
        }

        private static void AddThreat(DynamicBuffer<ThreatEntry> buf, Entity target, float amount, double now)
        {
            for (int i = 0; i < buf.Length; i++)
            {
                var e = buf[i];
                if (e.Target == target)
                {
                    e.Threat += amount;
                    e.LastAggroTime = now;
                    buf[i] = e;
                    return;
                }
            }

            buf.Add(new ThreatEntry
            {
                Target = target,
                Threat = amount,
                LastSeenTime = 0,
                LastAggroTime = now
            });
        }
    }
}

