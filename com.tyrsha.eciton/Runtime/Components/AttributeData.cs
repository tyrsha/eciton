using Unity.Entities;

namespace Tyrsha.Eciton
{
    public struct AttributeData : IComponentData
    {
        public float Health;
        public float Mana;
        public float Strength;
        public float Agility;
    }
}