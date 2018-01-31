// ==========================================================================
//  EdmModelBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Entities.Assets.State;

namespace Squidex.Domain.Apps.Entities.Assets.Edm
{
    public class EdmModelBuilder
    {
        private readonly IEdmModel edmModel;
        public EdmModelBuilder()
        {
            edmModel = BuildEdmModel();
        }

        public virtual IEdmModel EdmModel
        {
            get { return edmModel; }
        }

        private IEdmModel BuildEdmModel()
        {
            var model = new EdmModel();
            var container = new EdmEntityContainer("Squidex", "Container");
            var entityType = new EdmEntityType("Squidex", "Asset");

            entityType.AddStructuralProperty(nameof(AssetState.Id), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(AssetState.AppId), EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty(nameof(AssetState.Created), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(AssetState.CreatedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(AssetState.LastModified), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(AssetState.LastModifiedBy), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(AssetState.Version), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(AssetState.FileName), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(AssetState.FileSize), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(AssetState.FileVersion), EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty(nameof(AssetState.IsImage), EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty(nameof(AssetState.MimeType), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(AssetState.PixelHeight), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(AssetState.PixelWidth), EdmPrimitiveTypeKind.Int32);

            model.AddElement(container);
            model.AddElement(entityType);

            container.AddEntitySet("AssetSet", entityType);

            return model;
        }
    }
}
