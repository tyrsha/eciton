using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// Ability 정의(Authoring) 스텁. 런타임 코어는 UnityEngine에 의존하지 않으므로,
    /// 이 에셋은 프로젝트에서 Baker/변환을 통해 ID/Blob으로 내려보내는 용도다.
    /// </summary>
    public class AbilityDefinitionAsset : ScriptableObject
    {
        public int AbilityId;
        public float CooldownDuration;
        public float ManaCost;
    }
}

