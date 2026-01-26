using System.Collections.Generic;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// 태그 데이터베이스 ScriptableObject.
    /// 모든 태그 정의를 포함하며, Baker가 Blob을 생성할 때 사용한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Eciton/Tag Database", fileName = "EcitonTagDatabase")]
    public class TagDatabaseAsset : ScriptableObject
    {
        [Tooltip("사용할 비트마스크 크기")]
        public TagBitmaskSize BitmaskSize = TagBitmaskSize.Medium;

        [Tooltip("등록된 모든 태그 정의")]
        public TagDefinitionAsset[] Tags;

        /// <summary>
        /// 태그를 위상정렬하여 반환 (부모가 자식보다 먼저 오도록).
        /// 비트 인덱스 할당 시 부모의 비트가 먼저 할당되어야 closure를 계산할 수 있다.
        /// </summary>
        public List<TagDefinitionAsset> GetTopologicallySortedTags()
        {
            var result = new List<TagDefinitionAsset>();
            var visited = new HashSet<TagDefinitionAsset>();

            if (Tags == null) return result;

            foreach (var tag in Tags)
            {
                if (tag != null)
                    TopologicalSort(tag, visited, result);
            }

            return result;
        }

        private void TopologicalSort(TagDefinitionAsset tag, HashSet<TagDefinitionAsset> visited, List<TagDefinitionAsset> result)
        {
            if (tag == null || visited.Contains(tag))
                return;

            // 먼저 부모 방문
            if (tag.Parent != null)
                TopologicalSort(tag.Parent, visited, result);

            visited.Add(tag);
            result.Add(tag);
        }

        /// <summary>최대 태그 수 확인</summary>
        public int GetMaxTagCount()
        {
            return TagBitmaskUtility.GetMaxTagCount(BitmaskSize);
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Tags")]
        private void ValidateTags()
        {
            if (Tags == null || Tags.Length == 0)
            {
                Debug.LogWarning("No tags defined in database.");
                return;
            }

            int maxCount = GetMaxTagCount();
            var uniqueTags = new HashSet<TagDefinitionAsset>();
            var uniqueIds = new HashSet<int>();

            foreach (var tag in Tags)
            {
                if (tag == null)
                {
                    Debug.LogWarning("Null tag entry found.");
                    continue;
                }

                if (!uniqueTags.Add(tag))
                {
                    Debug.LogWarning($"Duplicate tag entry: {tag.TagName}");
                }

                if (!uniqueIds.Add(tag.TagId))
                {
                    Debug.LogWarning($"Duplicate TagId {tag.TagId} for tag: {tag.TagName}");
                }
            }

            int actualCount = uniqueTags.Count;
            if (actualCount > maxCount)
            {
                Debug.LogError($"Too many tags ({actualCount}) for bitmask size {BitmaskSize} (max {maxCount}). Consider using a larger bitmask size.");
            }
            else
            {
                Debug.Log($"Tag database validated: {actualCount} tags (max {maxCount} for {BitmaskSize})");
            }
        }

        [ContextMenu("Auto-assign Bit Indices")]
        private void AutoAssignBitIndices()
        {
            // 이 기능은 Editor 스크립트에서 더 복잡하게 구현 가능
            Debug.Log("Bit indices will be auto-assigned during baking based on topological order.");
        }
#endif
    }
}
