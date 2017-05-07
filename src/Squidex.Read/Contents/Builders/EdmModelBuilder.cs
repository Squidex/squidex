// ==========================================================================
//  EdmModelBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Squidex.Core;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Read.Schemas;
using Squidex.Read.Utils;

namespace Squidex.Read.Contents.Builders
{
    public sealed class EdmModelBuilder : CachingProviderBase
    {
        public EdmModelBuilder(IMemoryCache cache) 
            : base(cache)
        {
        }

        public IEdmModel BuildEdmModel(ISchemaEntityWithSchema schemaEntity, LanguagesConfig languagesConfig)
        {
            Guard.NotNull(languagesConfig, nameof(languagesConfig));
            Guard.NotNull(schemaEntity, nameof(schemaEntity));

            var isoCodes = string.Join(",", languagesConfig.Select(x => x.Language.Iso2Code).OrderBy(x => x));

            var cacheKey = $"{schemaEntity.Id}_{schemaEntity.Version}_{isoCodes}";

            var result = Cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60);

                return BuildEdmModel(schemaEntity.Schema, languagesConfig);
            });

            return result;
        }

        private static EdmModel BuildEdmModel(Schema schema, LanguagesConfig languagesConfig)
        {
            var model = new EdmModel();

            var container = new EdmEntityContainer("Squidex", "Container");

            var schemaType = schema.BuildEdmType(languagesConfig, x =>
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
