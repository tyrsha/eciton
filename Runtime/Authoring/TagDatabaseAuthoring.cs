using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Tyrsha.Eciton.Authoring
{
    /// <summary>
    /// 태그 데이터베이스를 런타임 Blob으로 베이크하는 Authoring 컴포넌트.
    /// 씬/서브씬에 배치하면 Baker가 Blob을 생성한다.
    /// </summary>
    public class TagDatabaseAuthoring : MonoBehaviour
    {
        [Tooltip("태그 데이터베이스 에셋")]
        public TagDatabaseAsset DatabaseAsset;
    }

    public class TagDatabaseBaker : Baker<TagDatabaseAuthoring>
    {
        public override void Bake(TagDatabaseAuthoring authoring)
        {
            if (authoring.DatabaseAsset == null || authoring.DatabaseAsset.Tags == null)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            var sortedTags = authoring.DatabaseAsset.GetTopologicallySortedTags();

            if (sortedTags.Count == 0)
                return;

            // 비트마스크 크기에 따라 적절한 Blob 생성
            switch (authoring.DatabaseAsset.BitmaskSize)
            {
                case TagBitmaskSize.Small:
                    BakeDatabase32(entity, sortedTags);
                    break;
                case TagBitmaskSize.Medium:
                    BakeDatabase64(entity, sortedTags);
                    break;
                case TagBitmaskSize.Large:
                    BakeDatabase128(entity, sortedTags);
                    break;
                case TagBitmaskSize.Huge:
                    BakeDatabase256(entity, sortedTags);
                    break;
            }
        }

        private void BakeDatabase32(Entity entity, List<TagDefinitionAsset> sortedTags)
        {
            using var builder = new BlobBuilder(AllocatorManager.Persistent);
            ref var root = ref builder.ConstructRoot<TagDatabaseBlob32>();

            // Find max TagId for sparse array
            int maxTagId = 0;
            foreach (var tag in sortedTags)
            {
                if (tag.TagId > maxTagId) maxTagId = tag.TagId;
            }

            root.TagCount = sortedTags.Count;
            root.MaxTagId = maxTagId;

            var tags = builder.Allocate(ref root.Tags, sortedTags.Count);
            var tagIdToIndex = builder.Allocate(ref root.TagIdToIndex, maxTagId + 1);

            // Initialize all to -1 (not found)
            for (int i = 0; i <= maxTagId; i++)
            {
                tagIdToIndex[i] = -1;
            }

            // Build tag definitions with closure masks
            var tagMasks = new Dictionary<TagDefinitionAsset, TagBitmask32>();

            for (int i = 0; i < sortedTags.Count; i++)
            {
                var tag = sortedTags[i];
                var ownMask = TagBitmask32.FromBitIndex(i);

                // Closure = own + parent's closure
                var closureMask = ownMask;
                if (tag.Parent != null && tagMasks.TryGetValue(tag.Parent, out var parentClosure))
                {
                    closureMask = closureMask | parentClosure;
                }

                tagMasks[tag] = closureMask;

                tags[i] = new TagDefinition32
                {
                    TagId = tag.TagId,
                    BitIndex = i,
                    ParentTagId = tag.Parent != null ? tag.Parent.TagId : -1,
                    OwnMask = ownMask,
                    ClosureMask = closureMask
                };

                tagIdToIndex[tag.TagId] = i;
            }

            var blob = builder.CreateBlobAssetReference<TagDatabaseBlob32>(AllocatorManager.Persistent);
            AddComponent(entity, new TagDatabaseSingleton32 { Database = blob });
        }

        private void BakeDatabase64(Entity entity, List<TagDefinitionAsset> sortedTags)
        {
            using var builder = new BlobBuilder(AllocatorManager.Persistent);
            ref var root = ref builder.ConstructRoot<TagDatabaseBlob64>();

            int maxTagId = 0;
            foreach (var tag in sortedTags)
            {
                if (tag.TagId > maxTagId) maxTagId = tag.TagId;
            }

            root.TagCount = sortedTags.Count;
            root.MaxTagId = maxTagId;

            var tags = builder.Allocate(ref root.Tags, sortedTags.Count);
            var tagIdToIndex = builder.Allocate(ref root.TagIdToIndex, maxTagId + 1);

            for (int i = 0; i <= maxTagId; i++)
            {
                tagIdToIndex[i] = -1;
            }

            var tagMasks = new Dictionary<TagDefinitionAsset, TagBitmask64>();

            for (int i = 0; i < sortedTags.Count; i++)
            {
                var tag = sortedTags[i];
                var ownMask = TagBitmask64.FromBitIndex(i);

                var closureMask = ownMask;
                if (tag.Parent != null && tagMasks.TryGetValue(tag.Parent, out var parentClosure))
                {
                    closureMask = closureMask | parentClosure;
                }

                tagMasks[tag] = closureMask;

                tags[i] = new TagDefinition64
                {
                    TagId = tag.TagId,
                    BitIndex = i,
                    ParentTagId = tag.Parent != null ? tag.Parent.TagId : -1,
                    OwnMask = ownMask,
                    ClosureMask = closureMask
                };

                tagIdToIndex[tag.TagId] = i;
            }

            var blob = builder.CreateBlobAssetReference<TagDatabaseBlob64>(AllocatorManager.Persistent);
            AddComponent(entity, new TagDatabaseSingleton64 { Database = blob });
        }

        private void BakeDatabase128(Entity entity, List<TagDefinitionAsset> sortedTags)
        {
            using var builder = new BlobBuilder(AllocatorManager.Persistent);
            ref var root = ref builder.ConstructRoot<TagDatabaseBlob128>();

            int maxTagId = 0;
            foreach (var tag in sortedTags)
            {
                if (tag.TagId > maxTagId) maxTagId = tag.TagId;
            }

            root.TagCount = sortedTags.Count;
            root.MaxTagId = maxTagId;

            var tags = builder.Allocate(ref root.Tags, sortedTags.Count);
            var tagIdToIndex = builder.Allocate(ref root.TagIdToIndex, maxTagId + 1);

            for (int i = 0; i <= maxTagId; i++)
            {
                tagIdToIndex[i] = -1;
            }

            var tagMasks = new Dictionary<TagDefinitionAsset, TagBitmask128>();

            for (int i = 0; i < sortedTags.Count; i++)
            {
                var tag = sortedTags[i];
                var ownMask = TagBitmask128.FromBitIndex(i);

                var closureMask = ownMask;
                if (tag.Parent != null && tagMasks.TryGetValue(tag.Parent, out var parentClosure))
                {
                    closureMask = closureMask | parentClosure;
                }

                tagMasks[tag] = closureMask;

                tags[i] = new TagDefinition128
                {
                    TagId = tag.TagId,
                    BitIndex = i,
                    ParentTagId = tag.Parent != null ? tag.Parent.TagId : -1,
                    OwnMask = ownMask,
                    ClosureMask = closureMask
                };

                tagIdToIndex[tag.TagId] = i;
            }

            var blob = builder.CreateBlobAssetReference<TagDatabaseBlob128>(AllocatorManager.Persistent);
            AddComponent(entity, new TagDatabaseSingleton128 { Database = blob });
        }

        private void BakeDatabase256(Entity entity, List<TagDefinitionAsset> sortedTags)
        {
            using var builder = new BlobBuilder(AllocatorManager.Persistent);
            ref var root = ref builder.ConstructRoot<TagDatabaseBlob256>();

            int maxTagId = 0;
            foreach (var tag in sortedTags)
            {
                if (tag.TagId > maxTagId) maxTagId = tag.TagId;
            }

            root.TagCount = sortedTags.Count;
            root.MaxTagId = maxTagId;

            var tags = builder.Allocate(ref root.Tags, sortedTags.Count);
            var tagIdToIndex = builder.Allocate(ref root.TagIdToIndex, maxTagId + 1);

            for (int i = 0; i <= maxTagId; i++)
            {
                tagIdToIndex[i] = -1;
            }

            var tagMasks = new Dictionary<TagDefinitionAsset, TagBitmask256>();

            for (int i = 0; i < sortedTags.Count; i++)
            {
                var tag = sortedTags[i];
                var ownMask = TagBitmask256.FromBitIndex(i);

                var closureMask = ownMask;
                if (tag.Parent != null && tagMasks.TryGetValue(tag.Parent, out var parentClosure))
                {
                    closureMask = closureMask | parentClosure;
                }

                tagMasks[tag] = closureMask;

                tags[i] = new TagDefinition256
                {
                    TagId = tag.TagId,
                    BitIndex = i,
                    ParentTagId = tag.Parent != null ? tag.Parent.TagId : -1,
                    OwnMask = ownMask,
                    ClosureMask = closureMask
                };

                tagIdToIndex[tag.TagId] = i;
            }

            var blob = builder.CreateBlobAssetReference<TagDatabaseBlob256>(AllocatorManager.Persistent);
            AddComponent(entity, new TagDatabaseSingleton256 { Database = blob });
        }
    }
}
