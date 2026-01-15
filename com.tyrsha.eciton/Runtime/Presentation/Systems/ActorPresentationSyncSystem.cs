using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// 코어의 Attribute/Tag 상태를 PresentationState로 동기화하는 스텁 시스템.
    /// 여기서는 "렌더/뷰에서 쓰기 편한 값"으로만 복제한다.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameplayTagSystem))]
    public partial class ActorPresentationSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                ref ActorPresentationState presentation,
                in AttributeData attributes,
                in DynamicBuffer<GameplayTagElement> tags) =>
            {
                presentation.Health = attributes.Health;
                presentation.Shield = attributes.Shield;
                presentation.MoveSpeed = attributes.MoveSpeed;

                presentation.IsBurning = HasTag(tags, CommonIds.Tag_Burning);
                presentation.IsStunned = HasTag(tags, CommonIds.Tag_Stunned);
                presentation.IsSlowed = HasTag(tags, CommonIds.Tag_Slowed);
            }).ScheduleParallel();
        }

        private static bool HasTag(DynamicBuffer<GameplayTagElement> tags, int tagValue)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i].Tag.Value == tagValue)
                    return true;
            }
            return false;
        }
    }
}

