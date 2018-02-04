// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Edm
{
    public static class EdmAssetModel
    {
        public static readonly IEdmModel Edm;

        static EdmAssetModel()
        {
            var entityType = new EdmEntityType("Squidex", "Asset");

            entityType.AddStructuralProperty(nameof(IAssetEntity.Created).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IAssetEntity.CreatedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.LastModified).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IAssetEntity.LastModifiedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.Version).ToCamelCase(), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileName).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileSize).ToCamelCase(), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.FileVersion).ToCamelCase(), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(IAssetEntity.IsImage).ToCamelCase(), EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty(nameof(IAssetEntity.MimeType).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IAssetEntity.PixelHeight).ToCamelCase(), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(IAssetEntity.PixelWidth).ToCamelCase(), EdmPrimitiveTypeKind.Int32);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            Edm = model;
        }
    }
}
