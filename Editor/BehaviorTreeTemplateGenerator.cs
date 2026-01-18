using System.IO;
using Tyrsha.Eciton;
using Tyrsha.Eciton.Authoring;
using UnityEditor;
using UnityEngine;

namespace Tyrsha.Eciton.Editor
{
    public static class BehaviorTreeTemplateGenerator
    {
        [MenuItem("Eciton/AI/Create Behavior Tree Templates")]
        public static void CreateTemplates()
        {
            EnsureFolder("Assets", "Eciton");
            EnsureFolder("Assets/Eciton", "AI");
            EnsureFolder("Assets/Eciton/AI", "BehaviorTrees");

            CreateRangedChaseAndCast("Assets/Eciton/AI/BehaviorTrees/BT_Ranged_ChaseAndCast.asset", AbilityInputSlot.Slot1, 6f);
            CreateMeleeChaseAndAttack("Assets/Eciton/AI/BehaviorTrees/BT_Melee_ChaseAndAttack.asset", AbilityInputSlot.Slot1, 1.8f);
            CreateIdleIfNoTarget("Assets/Eciton/AI/BehaviorTrees/BT_Idle_IfNoTarget.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Eciton: BehaviorTree templates created under Assets/Eciton/AI/BehaviorTrees/");
        }

        [MenuItem("Eciton/Validate/Behavior Tree Assets")]
        public static void ValidateBehaviorTrees()
        {
            var guids = AssetDatabase.FindAssets("t:BehaviorTreeAsset");
            int errors = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BehaviorTreeAsset>(path);
                if (asset == null) continue;
                if (!ValidateOne(asset, path))
                    errors++;
            }

            if (guids.Length == 0)
            {
                Debug.Log("Eciton BT Validate: no BehaviorTreeAsset found.");
                return;
            }

            if (errors == 0)
                Debug.Log($"Eciton BT Validate: OK ({guids.Length} assets).");
            else
                Debug.LogError($"Eciton BT Validate: FAILED ({errors}/{guids.Length}).");
        }

        private static bool ValidateOne(BehaviorTreeAsset asset, string path)
        {
            if (asset.Nodes == null || asset.Nodes.Length == 0)
            {
                Debug.LogError($"[{path}] Nodes is empty.");
                return false;
            }

            bool ok = true;
            for (int i = 0; i < asset.Nodes.Length; i++)
            {
                var n = asset.Nodes[i];
                if (n == null)
                {
                    Debug.LogError($"[{path}] Nodes[{i}] is null.");
                    ok = false;
                    continue;
                }

                if (n.Children == null) continue;
                for (int c = 0; c < n.Children.Length; c++)
                {
                    int child = n.Children[c];
                    if (child < 0 || child >= asset.Nodes.Length)
                    {
                        Debug.LogError($"[{path}] Nodes[{i}] has invalid child index {child}.");
                        ok = false;
                    }
                }
            }

            return ok;
        }

        private static void CreateRangedChaseAndCast(string path, AbilityInputSlot slot, float stoppingDistance)
        {
            // Selector:
            //   Sequence(HasTarget, TargetInRange, ClearMove, PressSlot)
            //   Sequence(HasTarget, MoveToTarget(stoppingDistance))
            var nodes = new BehaviorTreeNodeAsset[7];
            nodes[0] = new BehaviorTreeNodeAsset { Type = BtNodeType.Selector, Children = new[] { 1, 5 } };

            nodes[1] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 2, 3, 4 } };
            nodes[2] = new BehaviorTreeNodeAsset { Type = BtNodeType.Condition, Condition = BtConditionType.HasTarget };
            nodes[3] = new BehaviorTreeNodeAsset { Type = BtNodeType.Condition, Condition = BtConditionType.TargetInRange };
            nodes[4] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 6 } };

            nodes[5] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 2, 6 } };
            nodes[6] = new BehaviorTreeNodeAsset { Type = BtNodeType.Action, Action = BtActionType.PressAbilitySlot, Slot = slot, FloatParam0 = stoppingDistance };

            CreateAsset(path, nodes);
        }

        private static void CreateMeleeChaseAndAttack(string path, AbilityInputSlot slot, float stoppingDistance)
        {
            // Selector:
            //   Sequence(HasTarget, TargetInRange, PressSlot)
            //   Sequence(HasTarget, MoveToTarget(stoppingDistance))
            var nodes = new BehaviorTreeNodeAsset[6];
            nodes[0] = new BehaviorTreeNodeAsset { Type = BtNodeType.Selector, Children = new[] { 1, 4 } };

            nodes[1] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 2, 3 } };
            nodes[2] = new BehaviorTreeNodeAsset { Type = BtNodeType.Condition, Condition = BtConditionType.HasTarget };
            nodes[3] = new BehaviorTreeNodeAsset { Type = BtNodeType.Action, Action = BtActionType.PressAbilitySlot, Slot = slot };

            nodes[4] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 2, 5 } };
            nodes[5] = new BehaviorTreeNodeAsset { Type = BtNodeType.Action, Action = BtActionType.MoveToTarget, FloatParam0 = stoppingDistance };

            CreateAsset(path, nodes);
        }

        private static void CreateIdleIfNoTarget(string path)
        {
            // Selector:
            //   Sequence(HasTarget, Success)
            //   ClearMove
            var nodes = new BehaviorTreeNodeAsset[4];
            nodes[0] = new BehaviorTreeNodeAsset { Type = BtNodeType.Selector, Children = new[] { 1, 3 } };
            nodes[1] = new BehaviorTreeNodeAsset { Type = BtNodeType.Sequence, Children = new[] { 2 } };
            nodes[2] = new BehaviorTreeNodeAsset { Type = BtNodeType.Condition, Condition = BtConditionType.HasTarget };
            nodes[3] = new BehaviorTreeNodeAsset { Type = BtNodeType.Action, Action = BtActionType.ClearMoveRequest };

            CreateAsset(path, nodes);
        }

        private static void CreateAsset(string path, BehaviorTreeNodeAsset[] nodes)
        {
            var asset = AssetDatabase.LoadAssetAtPath<BehaviorTreeAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BehaviorTreeAsset>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.Nodes = nodes;
            EditorUtility.SetDirty(asset);
        }

        private static void EnsureFolder(string parent, string name)
        {
            var p = Path.Combine(parent, name).Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(p))
                return;
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}

