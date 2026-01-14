using System.Text;
using Tyrsha.Eciton;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 실행 로그/이펙트 큐/버프(ActiveEffect) 상태를 IMGUI로 표시하는 디버그 HUD.
    /// 로직(코어)과 분리된 Presentation 전용.
    /// </summary>
    public class EcitonDebugHud : MonoBehaviour
    {
        [Header("UI")]
        public bool Visible = true;
        public int MaxEntitiesToShow = 32;
        public int MaxLogLines = 30;
        public Vector2 Scroll;

        private int _selectedIndex;
        private readonly StringBuilder _sb = new StringBuilder(16 * 1024);

        private void OnGUI()
        {
            if (!Visible)
                return;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                GUI.Label(new Rect(10, 10, 600, 40), "EcitonDebugHud: Default World not available.");
                return;
            }

            var em = world.EntityManager;
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<AbilitySystemComponent>());
            var entities = q.ToEntityArray(AllocatorManager.Temp);

            GUILayout.BeginArea(new Rect(10, 10, 720, Screen.height - 20), GUI.skin.window);
            GUILayout.Label($"Eciton Debug HUD (ASC entities: {entities.Length})");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(40)))
                _selectedIndex = Mathf.Max(0, _selectedIndex - 1);
            GUILayout.Label($"Selected: {_selectedIndex + 1}/{Mathf.Max(1, entities.Length)}", GUILayout.Width(160));
            if (GUILayout.Button(">", GUILayout.Width(40)))
                _selectedIndex = Mathf.Min(Mathf.Max(0, entities.Length - 1), _selectedIndex + 1);
            Visible = GUILayout.Toggle(Visible, "Visible", GUILayout.Width(90));
            GUILayout.EndHorizontal();

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, entities.Length - 1));
            Entity selected = entities.Length > 0 ? entities[_selectedIndex] : Entity.Null;

            Scroll = GUILayout.BeginScrollView(Scroll);
            _sb.Clear();

            if (selected == Entity.Null)
            {
                _sb.AppendLine("No ASC entity found.");
            }
            else
            {
                _sb.AppendLine($"Entity: {selected} (Index={selected.Index}, Version={selected.Version})");

                if (em.HasComponent<AttributeData>(selected))
                {
                    var a = em.GetComponentData<AttributeData>(selected);
                    _sb.AppendLine($"Attributes: HP={a.Health:0.##} Mana={a.Mana:0.##} Shield={a.Shield:0.##} MoveSpeed={a.MoveSpeed:0.##}");
                }

                // Tags
                _sb.Append("Tags: ");
                if (em.HasBuffer<GameplayTagElement>(selected))
                {
                    var tags = em.GetBuffer<GameplayTagElement>(selected);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (i > 0) _sb.Append(", ");
                        _sb.Append(tags[i].Tag.Value);
                    }
                }
                _sb.AppendLine();

                _sb.AppendLine();
                _sb.AppendLine("Effect Queue (pending requests):");
                DumpBufferCounts(em, selected, _sb);

                _sb.AppendLine();
                _sb.AppendLine("Active Buff/Debuff List:");
                if (em.HasBuffer<ActiveEffect>(selected))
                {
                    var active = em.GetBuffer<ActiveEffect>(selected);
                    if (active.Length == 0)
                        _sb.AppendLine("  (none)");
                    AbilityEffectDatabase db = default;
                    bool hasDb = TryGetDatabase(em, out db);
                    for (int i = 0; i < active.Length; i++)
                    {
                        var e = active[i];
                        bool found = false;
                        
                        if (hasDb && db.Blob.IsCreated)
                        {
                            ref var effects = ref db.Blob.Value.Effects;
                            for (int j = 0; j < effects.Length; j++)
                            {
                                if (effects[j].EffectId == e.EffectId)
                                {
                                    ref var def = ref effects[j];
                                    found = true;
                                    float remain = def.IsPermanent ? -1f : e.RemainingTime;
                                    _sb.AppendLine($"  - EffectId={e.EffectId} Remain={remain:0.##} Stack={e.StackCount} Tag={def.GrantedTag.Value} Periodic={def.IsPeriodic} Period={def.Period:0.##}");
                                    break;
                                }
                            }
                        }
                        
                        if (!found)
                        {
                            _sb.AppendLine($"  - EffectId={e.EffectId} Remain={e.RemainingTime:0.##} Stack={e.StackCount}");
                        }
                    }
                }

                _sb.AppendLine();
                _sb.AppendLine("Ability/Effect Log (recent):");
                DumpEventLog(em, selected, _sb);
            }

            GUILayout.TextArea(_sb.ToString());
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            entities.Dispose();
        }

        private static void DumpBufferCounts(EntityManager em, Entity e, StringBuilder sb)
        {
            sb.AppendLine($"  ApplyEffectByIdRequest: {GetLen<ApplyEffectByIdRequest>(em, e)}");
            sb.AppendLine($"  ApplyEffectRequest:     {GetLen<ApplyEffectRequest>(em, e)}");
            sb.AppendLine($"  RemoveEffectRequest:    {GetLen<RemoveEffectRequest>(em, e)}");
            sb.AppendLine($"  RemoveEffectsWithTag:   {GetLen<RemoveEffectsWithTagRequest>(em, e)}");
        }

        private void DumpEventLog(EntityManager em, Entity selected, StringBuilder sb)
        {
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<GameplayEventLogSingleton>());
            if (q.CalculateEntityCount() == 0)
            {
                sb.AppendLine("  (no GameplayEventLogSingleton)");
                return;
            }

            var logEntity = q.GetSingletonEntity();
            if (!em.HasBuffer<GameplayEventLogEntry>(logEntity))
            {
                sb.AppendLine("  (no GameplayEventLogEntry buffer)");
                return;
            }

            var log = em.GetBuffer<GameplayEventLogEntry>(logEntity);
            int shown = 0;
            for (int i = log.Length - 1; i >= 0 && shown < MaxLogLines; i--)
            {
                var entry = log[i];
                if (entry.Target != selected)
                    continue;
                sb.AppendLine($"  [{entry.Timestamp:0.00}] {entry.Type} id={entry.Id} mag={entry.Magnitude:0.##} src={entry.Source} tgt={entry.Target}");
                shown++;
            }

            if (shown == 0)
                sb.AppendLine("  (no entries for selected entity)");
        }

        private static int GetLen<T>(EntityManager em, Entity e) where T : unmanaged, IBufferElementData
        {
            return em.HasBuffer<T>(e) ? em.GetBuffer<T>(e).Length : 0;
        }

        private static bool TryGetDatabase(EntityManager em, out AbilityEffectDatabase db)
        {
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<AbilityEffectDatabase>());
            if (q.CalculateEntityCount() == 0)
            {
                db = default;
                return false;
            }

            db = q.GetSingleton<AbilityEffectDatabase>();
            return db.Blob.IsCreated;
        }
    }
}

