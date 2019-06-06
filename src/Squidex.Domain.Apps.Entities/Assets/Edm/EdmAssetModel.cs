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

            void AddProperty(string name, EdmPrimitiveTypeKind type)
            {
                entityType.AddStructuralProperty(name.ToCamelCase(), type);
            }

            AddProperty(nameof(IAssetEntity.Id), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Created), EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty(nameof(IAssetEntity.CreatedBy), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
            AddProperty(nameof(IAssetEntity.LastModifiedBy), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Version), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.FileName), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.FileHash), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.FileSize), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.FileVersion), EdmPrimitiveTypeKind.Int64);
            AddProperty(nameof(IAssetEntity.IsImage), EdmPrimitiveTypeKind.Boolean);
            AddProperty(nameof(IAssetEntity.MimeType), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.PixelHeight), EdmPrimitiveTypeKind.Int32);
            AddProperty(nameof(IAssetEntity.PixelWidth), EdmPrimitiveTypeKind.Int32);
            AddProperty(nameof(IAssetEntity.Slug), EdmPrimitiveTypeKind.String);
            AddProperty(nameof(IAssetEntity.Tags), EdmPrimitiveTypeKind.String);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("AssetSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            Edm = model;
        }
    }
}
