using Unity.Entities;

namespace Tyrsha.Eciton
{
    public struct AbilityBase : IComponentData
    {
        public bool IsActive;
        public Entity TargetEntity; // 능력이 영향을 미칠 엔티티
    }
}