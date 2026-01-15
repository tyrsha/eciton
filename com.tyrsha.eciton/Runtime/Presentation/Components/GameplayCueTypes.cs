using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>프레젠테이션 전용 GameplayCue 이벤트 타입.</summary>
    public enum GameplayCueEventType : byte
    {
        None = 0,
        TagAdded = 1,
        TagRemoved = 2,
    }

    /// <summary>GameplayCue 이벤트(코어->프레젠테이션 라우팅).</summary>
    [InternalBufferCapacity(16)]
    public struct GameplayCueEvent : IBufferElementData
    {
        public GameplayCueEventType Type;
        public GameplayTag Tag;
    }
}

