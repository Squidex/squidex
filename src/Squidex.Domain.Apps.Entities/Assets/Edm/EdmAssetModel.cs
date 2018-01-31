// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;

namespace Squidex.Domain.Apps.Entities.Assets.Edm
{
    public static class EdmAssetModel
    {
        public static readonly IEdmModel Edm;

        static EdmAssetModel()
        {
            var entityType = new EdmEntityType("Squidex", "Asset");

            entityType.AddStructuralProperty(nameof(IAssetEntity.Id), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(IAssetEntity.AppId), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(IAssetEntity.Created), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IAssetEntity.CreatedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IAssetEntity.LastModifiedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.Version), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileName), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileSize), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileVersion), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.IsImage), EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty(nameof(IAssetEntity.MimeType), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.PixelHeight), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(IAssetEntity.PixelWidth), EdmPrimitiveTypeKind.Int32);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            Edm = model;
        }
    }
}
