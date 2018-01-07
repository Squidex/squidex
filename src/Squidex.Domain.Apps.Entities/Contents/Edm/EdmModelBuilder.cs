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

            var container = new EdmEntityContainer("Squidex", "Container");

            var schemaType = schema.BuildEdmType(partitionResolver, x =>
            {
                model.AddElement(x);

                return x;
            });

            var entityType = new EdmEntityType("Squidex", schema.Name);
            entityType.AddStructuralProperty("data", new EdmComplexTypeReference(schemaType, false));
            entityType.AddStructuralProperty("version", EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty("created", EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty("createdBy", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("lastModified", EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty("lastModifiedBy", EdmPrimitiveTypeKind.String);

            model.AddElement(container);
            model.AddElement(schemaType);
            model.AddElement(entityType);

            container.AddEntitySet("ContentSet", entityType);

            return model;
        }
    }
}
