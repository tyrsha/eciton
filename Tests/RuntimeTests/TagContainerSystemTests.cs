using NUnit.Framework;
using Unity.Entities;

namespace Tyrsha.Eciton.Tests
{
    /// <summary>
    /// TagContainerSystem 통합 테스트.
    /// 태그 추가/제거 요청 처리, 스택 카운트, 비트마스크 재계산 검증.
    /// </summary>
    public class TagContainerSystemTests
    {
        private World _world;
        private EntityManager _em;
        private SystemHandle _tagSystem;

        // 테스트용 태그 마스크 (Parent Closure 시뮬레이션)
        // Status (bit 0)
        // Status.Debuff (bit 1) -> closure = bit 0 | bit 1
        // Status.Debuff.Stunned (bit 2) -> closure = bit 0 | bit 1 | bit 2
        // Status.Debuff.Slowed (bit 3) -> closure = bit 0 | bit 1 | bit 3
        private static readonly TagBitmask32 Status_Own = TagBitmask32.FromBitIndex(0);
        private static readonly TagBitmask32 Status_Closure = Status_Own;

        private static readonly TagBitmask32 Debuff_Own = TagBitmask32.FromBitIndex(1);
        private static readonly TagBitmask32 Debuff_Closure = Status_Own | Debuff_Own;

        private static readonly TagBitmask32 Stunned_Own = TagBitmask32.FromBitIndex(2);
        private static readonly TagBitmask32 Stunned_Closure = Status_Own | Debuff_Own | Stunned_Own;

        private static readonly TagBitmask32 Slowed_Own = TagBitmask32.FromBitIndex(3);
        private static readonly TagBitmask32 Slowed_Closure = Status_Own | Debuff_Own | Slowed_Own;

        [SetUp]
        public void SetUp()
        {
            _world = new World("TagContainerSystemTests");
            _em = _world.EntityManager;
            _tagSystem = _world.CreateSystem<TagContainerSystem32>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.IsCreated)
                _world.Dispose();
        }

        private Entity CreateEntityWithTagContainer()
        {
            var e = _em.CreateEntity();
            _em.AddComponentData(e, TagContainer32.Empty);
            _em.AddBuffer<ActiveTag32>(e);
            _em.AddBuffer<AddTagRequest32>(e);
            _em.AddBuffer<RemoveTagRequest32>(e);
            return e;
        }

        [Test]
        public void AddTagRequest_adds_tag_to_container()
        {
            var entity = CreateEntityWithTagContainer();

            // Stunned 태그 추가 요청
            _em.GetBuffer<AddTagRequest32>(entity).Add(new AddTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });

            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);
            var activeTags = _em.GetBuffer<ActiveTag32>(entity);

            // Stunned의 Own 비트가 있어야 함
            Assert.IsTrue(container.OwnTagsMask.ContainsAll(Stunned_Own));
            // Combined에는 closure (Status + Debuff + Stunned) 비트가 모두 있어야 함
            Assert.IsTrue(container.CombinedMask.ContainsAll(Stunned_Closure));
            Assert.AreEqual(1, activeTags.Length);
            Assert.AreEqual(1, activeTags[0].StackCount);
        }

        [Test]
        public void RemoveTagRequest_removes_tag_from_container()
        {
            var entity = CreateEntityWithTagContainer();

            // 먼저 태그 추가
            _em.GetBuffer<AddTagRequest32>(entity).Add(new AddTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });
            _tagSystem.Update(_world.Unmanaged);

            // 제거 요청
            _em.GetBuffer<RemoveTagRequest32>(entity).Add(new RemoveTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });
            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);
            var activeTags = _em.GetBuffer<ActiveTag32>(entity);

            Assert.IsTrue(container.OwnTagsMask.IsEmpty());
            Assert.IsTrue(container.CombinedMask.IsEmpty());
            Assert.AreEqual(0, activeTags.Length);
        }

        [Test]
        public void Multiple_tags_combine_in_container()
        {
            var entity = CreateEntityWithTagContainer();

            // Stunned와 Slowed 모두 추가
            var addBuffer = _em.GetBuffer<AddTagRequest32>(entity);
            addBuffer.Add(new AddTagRequest32 { OwnMask = Stunned_Own, ClosureMask = Stunned_Closure });
            addBuffer.Add(new AddTagRequest32 { OwnMask = Slowed_Own, ClosureMask = Slowed_Closure });

            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);
            var activeTags = _em.GetBuffer<ActiveTag32>(entity);

            // 두 태그의 own 비트가 모두 있어야 함
            Assert.IsTrue(container.OwnTagsMask.ContainsAll(Stunned_Own));
            Assert.IsTrue(container.OwnTagsMask.ContainsAll(Slowed_Own));

            // Combined는 두 closure의 합집합
            var expectedCombined = Stunned_Closure | Slowed_Closure;
            Assert.IsTrue(container.CombinedMask.ContainsAll(expectedCombined));

            Assert.AreEqual(2, activeTags.Length);
        }

        [Test]
        public void Same_tag_added_twice_increases_stack_count()
        {
            var entity = CreateEntityWithTagContainer();

            var addBuffer = _em.GetBuffer<AddTagRequest32>(entity);
            addBuffer.Add(new AddTagRequest32 { OwnMask = Stunned_Own, ClosureMask = Stunned_Closure });
            addBuffer.Add(new AddTagRequest32 { OwnMask = Stunned_Own, ClosureMask = Stunned_Closure });

            _tagSystem.Update(_world.Unmanaged);

            var activeTags = _em.GetBuffer<ActiveTag32>(entity);

            Assert.AreEqual(1, activeTags.Length);
            Assert.AreEqual(2, activeTags[0].StackCount);
        }

        [Test]
        public void Remove_with_stack_count_decrements_instead_of_removing()
        {
            var entity = CreateEntityWithTagContainer();

            // 2번 추가
            var addBuffer = _em.GetBuffer<AddTagRequest32>(entity);
            addBuffer.Add(new AddTagRequest32 { OwnMask = Stunned_Own, ClosureMask = Stunned_Closure });
            addBuffer.Add(new AddTagRequest32 { OwnMask = Stunned_Own, ClosureMask = Stunned_Closure });
            _tagSystem.Update(_world.Unmanaged);

            // 1번 제거
            _em.GetBuffer<RemoveTagRequest32>(entity).Add(new RemoveTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });
            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);
            var activeTags = _em.GetBuffer<ActiveTag32>(entity);

            // 스택이 1로 감소, 태그는 여전히 존재
            Assert.AreEqual(1, activeTags.Length);
            Assert.AreEqual(1, activeTags[0].StackCount);
            Assert.IsTrue(container.OwnTagsMask.ContainsAll(Stunned_Own));
        }

        [Test]
        public void HasTag_returns_true_for_parent_query_when_child_present()
        {
            var entity = CreateEntityWithTagContainer();

            // Stunned (자식) 태그 추가
            _em.GetBuffer<AddTagRequest32>(entity).Add(new AddTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });
            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);

            // Parent Closure 방식: Debuff 쿼리로 Stunned를 찾을 수 있어야 함
            // container.CombinedMask에 Debuff_Own 비트가 포함되어 있음 (Stunned_Closure에 포함)
            Assert.IsTrue(container.HasTag(Debuff_Closure));
            Assert.IsTrue(container.HasTag(Status_Closure));
            Assert.IsTrue(container.HasTag(Stunned_Closure));

            // Slowed는 없음
            Assert.IsFalse(container.HasExactTag(Slowed_Own));
        }

        [Test]
        public void Requests_are_cleared_after_processing()
        {
            var entity = CreateEntityWithTagContainer();

            _em.GetBuffer<AddTagRequest32>(entity).Add(new AddTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });

            _tagSystem.Update(_world.Unmanaged);

            var addRequests = _em.GetBuffer<AddTagRequest32>(entity);
            var removeRequests = _em.GetBuffer<RemoveTagRequest32>(entity);

            Assert.AreEqual(0, addRequests.Length);
            Assert.AreEqual(0, removeRequests.Length);
        }

        [Test]
        public void Container_extension_HasTagOrChild_works_correctly()
        {
            var entity = CreateEntityWithTagContainer();

            _em.GetBuffer<AddTagRequest32>(entity).Add(new AddTagRequest32
            {
                OwnMask = Stunned_Own,
                ClosureMask = Stunned_Closure
            });
            _tagSystem.Update(_world.Unmanaged);

            var container = _em.GetComponentData<TagContainer32>(entity);

            // HasTagOrChild는 부모 태그로 자식을 찾을 수 있음
            Assert.IsTrue(container.HasTagOrChild(Debuff_Closure));
            Assert.IsTrue(container.HasTagOrChild(Status_Closure));
        }
    }
}
