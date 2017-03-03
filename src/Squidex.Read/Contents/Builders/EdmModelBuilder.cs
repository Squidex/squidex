// ==========================================================================
//  EdmModelBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Read.Schemas;
using Squidex.Read.Utils;

namespace Squidex.Read.Contents.Builders
{
    public sealed class EdmModelBuilder : CachingProvider
    {
        public EdmModelBuilder(IMemoryCache cache) 
            : base(cache)
        {
        }

        public IEdmModel BuildEdmModel(ISchemaEntityWithSchema schemaEntity, HashSet<Language> languages)
        {
            Guard.NotNull(languages, nameof(languages));
            Guard.NotNull(schemaEntity, nameof(schemaEntity));

            var cacheKey = $"{schemaEntity.Id}_{schemaEntity.Version}_{string.Join(",", languages.Select(x => x.Iso2Code).OrderBy(x => x))}";

            var result = Cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60);

                return BuildEdmModel(schemaEntity.Schema, languages);
            });

            return result;
        }

        private static EdmModel BuildEdmModel(Schema schema, HashSet<Language> languages)
        {
            var model = new EdmModel();

            var container = new EdmEntityContainer("Squidex", "Container");

            var schemaType = schema.BuildEdmType(languages, x =>
            {
                model.AddElement(x);

                return x;
            });

            var entityType = new EdmEntityType("Squidex", schema.Name);
            entityType.AddStructuralProperty("data", new EdmComplexTypeReference(schemaType, false));
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
