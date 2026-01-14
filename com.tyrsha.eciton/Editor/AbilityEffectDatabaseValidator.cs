using System.Collections.Generic;
using Tyrsha.Eciton.Authoring;
using UnityEditor;
using UnityEngine;

namespace Tyrsha.Eciton.Editor
{
    public static class AbilityEffectDatabaseValidator
    {
        [MenuItem("Eciton/Validate/Ability & Effect Database Authoring")]
        public static void ValidateAll()
        {
            var guids = AssetDatabase.FindAssets("t:GameObject");
            int errorCount = 0;
            int checkedCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                var authoring = go.GetComponent<AbilityEffectDatabaseAuthoring>();
                if (authoring == null) continue;

                checkedCount++;
                var abilities = authoring.DatabaseAsset != null ? authoring.DatabaseAsset.Abilities : authoring.Abilities;
                var effects = authoring.DatabaseAsset != null ? authoring.DatabaseAsset.Effects : authoring.Effects;

                if (!ValidateOne(abilities, effects, path))
                    errorCount++;
            }

            // ScriptableObject(DB asset)도 검사
            var dbGuids = AssetDatabase.FindAssets("t:AbilityEffectDatabaseAsset");
            foreach (var guid in dbGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var db = AssetDatabase.LoadAssetAtPath<AbilityEffectDatabaseAsset>(path);
                if (db == null) continue;

                checkedCount++;
                if (!ValidateOne(db.Abilities, db.Effects, path))
                    errorCount++;
            }

            if (checkedCount == 0)
            {
                Debug.Log("Eciton DB Validate: no AbilityEffectDatabaseAuthoring/Asset found.");
                return;
            }

            if (errorCount == 0)
                Debug.Log($"Eciton DB Validate: OK ({checkedCount} authoring objects).");
            else
                Debug.LogError($"Eciton DB Validate: FAILED ({errorCount}/{checkedCount}).");
        }

        private static bool ValidateOne(AbilityDefinitionAsset[] abilities, EffectDefinitionAsset[] effects, string assetPath)
        {
            bool ok = true;

            var abilityIds = new HashSet<int>();
            var effectIds = new HashSet<int>();

            // Effects: duplicates
            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    var e = effects[i];
                    if (e == null)
                    {
                        Debug.LogWarning($"[{assetPath}] Effects[{i}] is null.");
                        continue;
                    }

                    if (!effectIds.Add(e.EffectId))
                    {
                        Debug.LogError($"[{assetPath}] Duplicate EffectId={e.EffectId} in '{e.name}'.");
                        ok = false;
                    }
                }
            }

            // Abilities: duplicates + missing referenced effect ids
            if (abilities != null)
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    var a = abilities[i];
                    if (a == null)
                    {
                        Debug.LogWarning($"[{assetPath}] Abilities[{i}] is null.");
                        continue;
                    }

                    if (!abilityIds.Add(a.AbilityId))
                    {
                        Debug.LogError($"[{assetPath}] Duplicate AbilityId={a.AbilityId} in '{a.name}'.");
                        ok = false;
                    }

                    if (a.PrimaryEffectId != 0 && !effectIds.Contains(a.PrimaryEffectId))
                    {
                        Debug.LogError($"[{assetPath}] AbilityId={a.AbilityId} references missing PrimaryEffectId={a.PrimaryEffectId}.");
                        ok = false;
                    }
                    if (a.SecondaryEffectId != 0 && !effectIds.Contains(a.SecondaryEffectId))
                    {
                        Debug.LogError($"[{assetPath}] AbilityId={a.AbilityId} references missing SecondaryEffectId={a.SecondaryEffectId}.");
                        ok = false;
                    }
                }
            }

            return ok;
        }
    }
}

