// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    internal static class Fields
    {
        private static readonly Lazy<string> AssetIdField = new Lazy<string>(GetAssetIdField);
        private static readonly Lazy<string> AssetFolderIdField = new Lazy<string>(GetAssetFolderIdField);

        public static string AssetId => AssetIdField.Value;

        public static string AssetFolderId => AssetFolderIdField.Value;

        private static string GetAssetIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoAssetEntity)).GetMemberMap(nameof(MongoAssetEntity.Id)).ElementName;
        }

        private static string GetAssetFolderIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoAssetFolderEntity)).GetMemberMap(nameof(MongoAssetFolderEntity.Id)).ElementName;
        }
    }
}
