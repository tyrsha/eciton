using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// GAS의 GameplayEffectContext에 해당하는 최소 스텁.
    /// (피격 정보/랜덤 시드/히트 포인트 등은 이후 확장)
    /// </summary>
    public struct GameplayEffectContext
    {
        public Entity Instigator;
        public Entity Causer;
    }
}

