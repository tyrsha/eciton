using Unity.Entities;

namespace Tyrsha.Eciton
{
    public struct AbilityTag : IComponentData
    {
        public int AbilityId; // 특정 능력을 식별하기 위한 ID
    }
}