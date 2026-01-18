using System;
using Tyrsha.Eciton;
using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    [Serializable]
    public struct MonsterAbilitySlotEntry
    {
        public AbilityInputSlot Slot;
        public int AbilityId;
        public int Level;
    }

    /// <summary>
    /// 몬스터에게 능력을 부여하고(GrantAbilityRequest), 슬롯 바인딩(AbilityId 기반)을 설정하는 Authoring.
    /// </summary>
    public class MonsterAbilityLoadoutAuthoring : MonoBehaviour
    {
        public MonsterAbilitySlotEntry[] Slots;
    }

    public class MonsterAbilityLoadoutBaker : Baker<MonsterAbilityLoadoutAuthoring>
    {
        public override void Bake(MonsterAbilityLoadoutAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var grants = AddBuffer<GrantAbilityRequest>(entity);
            var desired = AddBuffer<AbilityInputBindingByAbilityId>(entity);
            AddBuffer<AbilityInputBinding>(entity);

            if (authoring.Slots == null)
                return;

            for (int i = 0; i < authoring.Slots.Length; i++)
            {
                var s = authoring.Slots[i];
                if (s.AbilityId == 0)
                    continue;

                desired.Add(new AbilityInputBindingByAbilityId
                {
                    Slot = s.Slot,
                    AbilityId = s.AbilityId
                });

                grants.Add(new GrantAbilityRequest
                {
                    AbilityId = s.AbilityId,
                    Level = s.Level <= 0 ? 1 : s.Level,
                    Source = entity
                });
            }
        }
    }
}

