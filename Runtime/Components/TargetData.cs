using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 타겟 지정 데이터 스텁. 현재는 단일 Entity만 지원.
    /// (다중/영역/라인/콘 등은 버퍼/Blob으로 확장)
    /// </summary>
    public struct TargetData
    {
        public Entity Target;
    }
}

