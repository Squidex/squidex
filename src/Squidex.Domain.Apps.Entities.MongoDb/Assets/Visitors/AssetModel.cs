// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors
{
    public static class AssetModel
    {
        public static readonly IEdmModel Edm;

        static AssetModel()
        {
            var entityType = new EdmEntityType("Squidex", "Asset");

            entityType.AddStructuralProperty(nameof(MongoAssetEntity.Id), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.AppId), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.Created), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.CreatedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.LastModifiedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.Version), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.FileName), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.FileSize), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.FileVersion), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.IsImage), EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.MimeType), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.PixelHeight), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(MongoAssetEntity.PixelWidth), EdmPrimitiveTypeKind.Int32);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            Edm = model;
        }
    }
}
