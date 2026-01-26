using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// 개별 태그 정의를 위한 ScriptableObject.
    /// 에디터에서 태그 계층 구조를 설정할 수 있다.
    ///
    /// 예시 계층:
    /// - Status (루트)
    ///   - Status.Buff
    ///     - Status.Buff.Shield
    ///     - Status.Buff.Haste
    ///   - Status.Debuff
    ///     - Status.Debuff.Stunned
    ///     - Status.Debuff.Slowed
    ///     - Status.Debuff.Burning
    /// </summary>
    [CreateAssetMenu(menuName = "Eciton/Tag Definition", fileName = "NewTag")]
    public class TagDefinitionAsset : ScriptableObject
    {
        [Tooltip("태그의 고유 ID (자동 생성 또는 수동 지정)")]
        public int TagId;

        [Tooltip("태그 이름 (예: Status.Debuff.Stunned)")]
        public string TagName;

        [Tooltip("부모 태그 (없으면 루트)")]
        public TagDefinitionAsset Parent;

        [Tooltip("태그 설명")]
        [TextArea(2, 4)]
        public string Description;

        /// <summary>전체 경로 이름 생성 (재귀)</summary>
        public string GetFullPath()
        {
            if (Parent == null)
                return TagName;
            return Parent.GetFullPath() + "." + TagName;
        }

        /// <summary>계층 깊이 반환 (루트 = 0)</summary>
        public int GetDepth()
        {
            if (Parent == null)
                return 0;
            return Parent.GetDepth() + 1;
        }

        /// <summary>모든 부모 태그 수집 (자신 포함)</summary>
        public void CollectAncestors(List<TagDefinitionAsset> result)
        {
            result.Add(this);
            if (Parent != null)
                Parent.CollectAncestors(result);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 자동 ID 생성 (이름 해시 기반)
            if (TagId == 0 && !string.IsNullOrEmpty(TagName))
            {
                TagId = GetFullPath().GetHashCode();
                if (TagId == 0) TagId = 1; // 0은 Invalid
            }
        }

        [ContextMenu("Generate ID from Name")]
        private void GenerateIdFromName()
        {
            if (!string.IsNullOrEmpty(TagName))
            {
                TagId = GetFullPath().GetHashCode();
                if (TagId == 0) TagId = 1;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
