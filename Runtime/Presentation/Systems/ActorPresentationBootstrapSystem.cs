using Unity.Entities;
using Tyrsha.Eciton;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// AbilitySystemComponent(ASC)을 가진 엔티티에 Presentation 상태 컴포넌트를 부착하는 스텁.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ActorPresentationBootstrapSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            // 스텁: ASC가 있고 PresentationState가 없는 엔티티에 추가
            Entities.WithoutBurst().WithNone<ActorPresentationState>().ForEach((Entity e, in AbilitySystemComponent asc) =>
            {
                _ = asc;
                em.AddComponentData(e, default(ActorPresentationState));
            }).Run();
        }
    }
}

