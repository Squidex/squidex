// ==========================================================================
//  EdmModelBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Edm
{
    public class EdmModelBuilder : CachingProviderBase
    {
        public EdmModelBuilder(IMemoryCache cache)
            : base(cache)
        {
        }

        public virtual IEdmModel BuildEdmModel(IAssetEntity asset)
        {
            Guard.NotNull(asset, nameof(asset));

            var cacheKey = $"Assets_EdmModel";

            var result = Cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                var model = new EdmModel();

                var container = new EdmEntityContainer("Squidex", "Container");

                var entityType = new EdmEntityType("Squidex", "Asset");
                entityType.AddStructuralProperty(nameof(asset.Id), EdmPrimitiveTypeKind.Guid);
                entityType.AddStructuralProperty(nameof(asset.AppId), EdmPrimitiveTypeKind.Guid);
                entityType.AddStructuralProperty(nameof(asset.Created), EdmPrimitiveTypeKind.DateTimeOffset);
                entityType.AddStructuralProperty(nameof(asset.CreatedBy), EdmPrimitiveTypeKind.String);
                entityType.AddStructuralProperty(nameof(asset.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
                entityType.AddStructuralProperty(nameof(asset.LastModifiedBy), EdmPrimitiveTypeKind.String);
                entityType.AddStructuralProperty(nameof(asset.Version), EdmPrimitiveTypeKind.Int64);

                entityType.AddStructuralProperty(nameof(asset.FileName), EdmPrimitiveTypeKind.String);
                entityType.AddStructuralProperty(nameof(asset.FileSize), EdmPrimitiveTypeKind.Int64);
                entityType.AddStructuralProperty(nameof(asset.FileVersion), EdmPrimitiveTypeKind.Int64);
                entityType.AddStructuralProperty(nameof(asset.IsImage), EdmPrimitiveTypeKind.Boolean);
                entityType.AddStructuralProperty(nameof(asset.MimeType), EdmPrimitiveTypeKind.String);
                entityType.AddStructuralProperty(nameof(asset.PixelHeight), EdmPrimitiveTypeKind.Int32);
                entityType.AddStructuralProperty(nameof(asset.PixelWidth), EdmPrimitiveTypeKind.Int32);

                model.AddElement(container);
                model.AddElement(entityType);

                container.AddEntitySet("AssetSet", entityType);

                return model;
            });

            return result;
        }
    }
}
