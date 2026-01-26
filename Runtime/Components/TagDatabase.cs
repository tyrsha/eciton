using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Tyrsha.Eciton
{
    #region Blob Structures

    /// <summary>
    /// 태그 데이터베이스 Blob (32비트).
    /// 런타임에서 TagId로 태그 정의를 조회하는데 사용.
    /// </summary>
    public struct TagDatabaseBlob32
    {
        /// <summary>모든 태그 정의 배열</summary>
        public BlobArray<TagDefinition32> Tags;

        /// <summary>TagId를 인덱스로 매핑 (sparse array)</summary>
        public BlobArray<int> TagIdToIndex;

        /// <summary>데이터베이스에 등록된 태그 수</summary>
        public int TagCount;

        /// <summary>최대 TagId 값</summary>
        public int MaxTagId;
    }

    public struct TagDatabaseBlob64
    {
        public BlobArray<TagDefinition64> Tags;
        public BlobArray<int> TagIdToIndex;
        public int TagCount;
        public int MaxTagId;
    }

    public struct TagDatabaseBlob128
    {
        public BlobArray<TagDefinition128> Tags;
        public BlobArray<int> TagIdToIndex;
        public int TagCount;
        public int MaxTagId;
    }

    public struct TagDatabaseBlob256
    {
        public BlobArray<TagDefinition256> Tags;
        public BlobArray<int> TagIdToIndex;
        public int TagCount;
        public int MaxTagId;
    }

    #endregion

    #region Lookup Structures

    /// <summary>
    /// 태그 데이터베이스 조회 유틸리티 (32비트).
    /// </summary>
    [BurstCompile]
    public struct TagDatabaseLookup32
    {
        [ReadOnly] public BlobAssetReference<TagDatabaseBlob32> Database;

        public bool IsCreated => Database.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTag(int tagId, out TagDefinition32 definition)
        {
            if (!IsCreated || tagId <= 0 || tagId > Database.Value.MaxTagId)
            {
                definition = TagDefinition32.Invalid;
                return false;
            }

            ref var blob = ref Database.Value;
            int index = blob.TagIdToIndex[tagId];
            if (index < 0)
            {
                definition = TagDefinition32.Invalid;
                return false;
            }

            definition = blob.Tags[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagDefinition32 GetTag(int tagId)
        {
            TryGetTag(tagId, out var definition);
            return definition;
        }

        /// <summary>TagId로 ClosureMask를 직접 조회</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask32 GetClosureMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.ClosureMask;
            return TagBitmask32.Empty;
        }

        /// <summary>TagId로 OwnMask를 직접 조회</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask32 GetOwnMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.OwnMask;
            return TagBitmask32.Empty;
        }

        /// <summary>등록된 태그 수</summary>
        public int TagCount => IsCreated ? Database.Value.TagCount : 0;
    }

    [BurstCompile]
    public struct TagDatabaseLookup64
    {
        [ReadOnly] public BlobAssetReference<TagDatabaseBlob64> Database;

        public bool IsCreated => Database.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTag(int tagId, out TagDefinition64 definition)
        {
            if (!IsCreated || tagId <= 0 || tagId > Database.Value.MaxTagId)
            {
                definition = TagDefinition64.Invalid;
                return false;
            }

            ref var blob = ref Database.Value;
            int index = blob.TagIdToIndex[tagId];
            if (index < 0)
            {
                definition = TagDefinition64.Invalid;
                return false;
            }

            definition = blob.Tags[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagDefinition64 GetTag(int tagId)
        {
            TryGetTag(tagId, out var definition);
            return definition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask64 GetClosureMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.ClosureMask;
            return TagBitmask64.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask64 GetOwnMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.OwnMask;
            return TagBitmask64.Empty;
        }

        public int TagCount => IsCreated ? Database.Value.TagCount : 0;
    }

    [BurstCompile]
    public struct TagDatabaseLookup128
    {
        [ReadOnly] public BlobAssetReference<TagDatabaseBlob128> Database;

        public bool IsCreated => Database.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTag(int tagId, out TagDefinition128 definition)
        {
            if (!IsCreated || tagId <= 0 || tagId > Database.Value.MaxTagId)
            {
                definition = TagDefinition128.Invalid;
                return false;
            }

            ref var blob = ref Database.Value;
            int index = blob.TagIdToIndex[tagId];
            if (index < 0)
            {
                definition = TagDefinition128.Invalid;
                return false;
            }

            definition = blob.Tags[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagDefinition128 GetTag(int tagId)
        {
            TryGetTag(tagId, out var definition);
            return definition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask128 GetClosureMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.ClosureMask;
            return TagBitmask128.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask128 GetOwnMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.OwnMask;
            return TagBitmask128.Empty;
        }

        public int TagCount => IsCreated ? Database.Value.TagCount : 0;
    }

    [BurstCompile]
    public struct TagDatabaseLookup256
    {
        [ReadOnly] public BlobAssetReference<TagDatabaseBlob256> Database;

        public bool IsCreated => Database.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTag(int tagId, out TagDefinition256 definition)
        {
            if (!IsCreated || tagId <= 0 || tagId > Database.Value.MaxTagId)
            {
                definition = TagDefinition256.Invalid;
                return false;
            }

            ref var blob = ref Database.Value;
            int index = blob.TagIdToIndex[tagId];
            if (index < 0)
            {
                definition = TagDefinition256.Invalid;
                return false;
            }

            definition = blob.Tags[index];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagDefinition256 GetTag(int tagId)
        {
            TryGetTag(tagId, out var definition);
            return definition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask256 GetClosureMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.ClosureMask;
            return TagBitmask256.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagBitmask256 GetOwnMask(int tagId)
        {
            if (TryGetTag(tagId, out var definition))
                return definition.OwnMask;
            return TagBitmask256.Empty;
        }

        public int TagCount => IsCreated ? Database.Value.TagCount : 0;
    }

    #endregion

    #region Singleton Components

    /// <summary>
    /// 태그 데이터베이스 싱글톤 컴포넌트 (32비트).
    /// </summary>
    public struct TagDatabaseSingleton32 : IComponentData
    {
        public BlobAssetReference<TagDatabaseBlob32> Database;

        public TagDatabaseLookup32 GetLookup()
        {
            return new TagDatabaseLookup32 { Database = Database };
        }
    }

    public struct TagDatabaseSingleton64 : IComponentData
    {
        public BlobAssetReference<TagDatabaseBlob64> Database;

        public TagDatabaseLookup64 GetLookup()
        {
            return new TagDatabaseLookup64 { Database = Database };
        }
    }

    public struct TagDatabaseSingleton128 : IComponentData
    {
        public BlobAssetReference<TagDatabaseBlob128> Database;

        public TagDatabaseLookup128 GetLookup()
        {
            return new TagDatabaseLookup128 { Database = Database };
        }
    }

    public struct TagDatabaseSingleton256 : IComponentData
    {
        public BlobAssetReference<TagDatabaseBlob256> Database;

        public TagDatabaseLookup256 GetLookup()
        {
            return new TagDatabaseLookup256 { Database = Database };
        }
    }

    #endregion
}
