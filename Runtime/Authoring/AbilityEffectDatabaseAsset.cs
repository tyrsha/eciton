using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// 빌드(베이킹)에서 Blob DB를 생성하기 위한 데이터베이스 ScriptableObject.
    /// 이 에셋을 참조하는 Authoring(MonoBehaviour)을 씬/서브씬에 두면 Baker가 Blob을 만든다.
    /// </summary>
    [CreateAssetMenu(menuName = "Eciton/Ability Effect Database", fileName = "EcitonAbilityEffectDatabase")]
    public class AbilityEffectDatabaseAsset : ScriptableObject
    {
        public AbilityDefinitionAsset[] Abilities;
        public EffectDefinitionAsset[] Effects;
    }
}

