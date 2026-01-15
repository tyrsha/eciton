using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    public struct GameplayEventLogSingleton : IComponentData { }

    [InternalBufferCapacity(128)]
    public struct GameplayEventLogEntry : IBufferElementData
    {
        public double Timestamp;
        public GameplayEventType Type;
        public Entity Source;
        public Entity Target;
        public int Id;
        public float Magnitude;
    }
}

