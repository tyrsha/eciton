using Unity.Entities;

namespace Tyrsha.Eciton
{
    /// <summary>
    /// 요청한 시나리오를 "월드 안에서" 바로 재현하는 최소 부트스트랩:
    /// Actor1이 Fireball을 발사 -> 투사체 비행 -> Actor2 충돌/폭발 ->
    /// Actor2 즉시 데미지 + 화상 DoT 적용.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(FireballAbilitySystem))]
    public class FireballScenarioBootstrapSystem : SystemBase
    {
        private bool _initialized;

        protected override void OnUpdate()
        {
            if (_initialized)
                return;

            _initialized = true;

            var em = EntityManager;

            var actor1 = em.CreateEntity();
            em.AddComponentData(actor1, new AbilitySystemComponent { Owner = actor1, Avatar = actor1 });
            em.AddComponentData(actor1, new AttributeData { Health = 100f, Mana = 100f, Strength = 10f, Agility = 10f });
            EnsureAscBuffers(em, actor1);

            var actor2 = em.CreateEntity();
            em.AddComponentData(actor2, new AbilitySystemComponent { Owner = actor2, Avatar = actor2 });
            em.AddComponentData(actor2, new AttributeData { Health = 100f, Mana = 50f, Strength = 8f, Agility = 8f });
            EnsureAscBuffers(em, actor2);

            // Actor1에게 Fireball 부여(데이터 드리븐)
            em.GetBuffer<GrantAbilityRequest>(actor1).Add(new GrantAbilityRequest
            {
                AbilityId = ExampleIds.Ability_Fireball,
                Level = 1,
                Source = actor1,
            });

            // Actor1이 Actor2를 타겟으로 Fireball 활성화 요청
            var tryActivate = em.GetBuffer<TryActivateAbilityRequest>(actor1);
            // NOTE: Handle은 Grant 이후 생성되므로, 스텁에서는 Handle=1을 가정(AbilityGrantSystem의 첫 핸들).
            // 실제 게임에서는 입력/AI가 GrantedAbility 목록에서 핸들을 조회해 요청을 생성한다.
            tryActivate.Add(new TryActivateAbilityRequest { Handle = new AbilityHandle { Value = 1, Version = 1 }, Target = actor2, TargetData = new TargetData { Target = actor2 } });
        }

        private static void EnsureAscBuffers(EntityManager em, Entity entity)
        {
            if (!em.HasBuffer<GrantedAbility>(entity)) em.AddBuffer<GrantedAbility>(entity);
            if (!em.HasBuffer<GrantAbilityRequest>(entity)) em.AddBuffer<GrantAbilityRequest>(entity);
            if (!em.HasBuffer<TryActivateAbilityRequest>(entity)) em.AddBuffer<TryActivateAbilityRequest>(entity);
            if (!em.HasBuffer<CancelAbilityRequest>(entity)) em.AddBuffer<CancelAbilityRequest>(entity);

            if (!em.HasBuffer<ApplyEffectRequest>(entity)) em.AddBuffer<ApplyEffectRequest>(entity);
            if (!em.HasBuffer<ApplyEffectByIdRequest>(entity)) em.AddBuffer<ApplyEffectByIdRequest>(entity);
            if (!em.HasBuffer<RemoveEffectRequest>(entity)) em.AddBuffer<RemoveEffectRequest>(entity);
            if (!em.HasBuffer<ActiveEffect>(entity)) em.AddBuffer<ActiveEffect>(entity);

            if (!em.HasBuffer<ApplyAttributeModifierRequest>(entity)) em.AddBuffer<ApplyAttributeModifierRequest>(entity);
            if (!em.HasBuffer<PendingGameplayEvent>(entity)) em.AddBuffer<PendingGameplayEvent>(entity);

            if (!em.HasBuffer<GameplayTagElement>(entity)) em.AddBuffer<GameplayTagElement>(entity);
            if (!em.HasBuffer<AddGameplayTagRequest>(entity)) em.AddBuffer<AddGameplayTagRequest>(entity);
            if (!em.HasBuffer<RemoveGameplayTagRequest>(entity)) em.AddBuffer<RemoveGameplayTagRequest>(entity);
        }
    }
}

