using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GAS의 AbilitySystemComponent(ASC) 역할을 하는 ECS 컴포넌트 스텁.
    /// 이 엔티티에 Ability/Effect/Tag 관련 버퍼들을 붙여서 사용한다.
    /// </summary>
    public struct AbilitySystemComponent : IComponentData
    {
        /// <summary>이 ASC를 소유한 엔티티(예: 플레이어/AI 컨트롤러 등).</summary>
        public Entity Owner;

        /// <summary>실제 게임플레이 표현/피격 대상이 되는 엔티티(예: 캐릭터 본체).</summary>
        public Entity Avatar;
    }
}

