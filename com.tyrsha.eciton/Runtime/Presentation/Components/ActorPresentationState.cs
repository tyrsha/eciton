using Unity.Entities;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어 상태를 렌더/UI 쪽에서 쓰기 쉽게 복제해두는 Presentation 전용 상태.
    /// (코어는 이 타입을 참조하지 않는다)
    /// </summary>
    public struct ActorPresentationState : IComponentData
    {
        public float Health;
        public float Shield;
        public float MoveSpeed;

        public bool IsBurning;
        public bool IsStunned;
        public bool IsSlowed;
    }
}

