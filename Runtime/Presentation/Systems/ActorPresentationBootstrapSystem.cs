using Unity.Entities;
using Tyrsha.Eciton;
using Unity.Collections;

namespace Tyrsha.Eciton.Presentation
{
    /// <summary>
    /// AbilitySystemComponent(ASC)을 가진 엔티티에 Presentation 상태 컴포넌트를 부착하는 스텁.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ActorPresentationBootstrapSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            // 스텁: ASC가 있고 PresentationState가 없는 엔티티에 추가
            var query = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<AbilitySystemComponent>() },
                None = new[] { ComponentType.ReadOnly<ActorPresentationState>() }
            });
            using var entities = query.ToEntityArray(AllocatorManager.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var e = entities[i];
                _ = em.GetComponentData<AbilitySystemComponent>(e);
                em.AddComponentData(e, default(ActorPresentationState));
            }
        }
    }
}

