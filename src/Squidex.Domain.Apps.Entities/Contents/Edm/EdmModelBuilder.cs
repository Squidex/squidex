// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.GenerateEdmSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Edm
{
    public class EdmModelBuilder : CachingProviderBase
    {
        public EdmModelBuilder(IMemoryCache cache)
            : base(cache)
        {
        }

        public virtual IEdmModel BuildEdmModel(ISchemaEntity schema, IAppEntity app)
        {
            Guard.NotNull(schema, nameof(schema));

            var cacheKey = $"{schema.Id}_{schema.Version}_{app.Id}_{app.Version}";

            var result = Cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60);

                return BuildEdmModel(schema.SchemaDef, app.PartitionResolver());
            });

            return result;
        }

        private static EdmModel BuildEdmModel(Schema schema, PartitionResolver partitionResolver)
        {
            var model = new EdmModel();

            var schemaType = schema.BuildEdmType(partitionResolver, x =>
            {
                model.AddElement(x);

                return x;
            });

            var entityType = new EdmEntityType("Squidex", schema.Name);
            entityType.AddStructuralProperty(nameof(IContentEntity.Created).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.CreatedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModified).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModifiedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Version).ToCamelCase(), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(IContentEntity.Data).ToCamelCase(), new EdmComplexTypeReference(schemaType, false));

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("ContentSet", entityType);

            model.AddElement(container);
            model.AddElement(schemaType);
            model.AddElement(entityType);

            return model;
        }
    }
}
