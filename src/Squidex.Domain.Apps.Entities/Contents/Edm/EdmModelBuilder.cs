// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
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
        private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(60);

        public EdmModelBuilder(IMemoryCache cache)
            : base(cache)
        {
        }

        public virtual IEdmModel BuildEdmModel(IAppEntity app, ISchemaEntity schema, bool withHidden)
        {
            Guard.NotNull(schema, nameof(schema));

            var cacheKey = BuildCacheKey(app, schema, withHidden);

            var result = Cache.GetOrCreate<IEdmModel>(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTime;

                return BuildEdmModel(schema.SchemaDef, app, withHidden);
            });

            return result;
        }

        private static EdmModel BuildEdmModel(Schema schema, IAppEntity app, bool withHidden)
        {
            var model = new EdmModel();

            var pascalAppName = app.Name.ToPascalCase();
            var pascalSchemaName = schema.Name.ToPascalCase();

            var typeFactory = new EdmTypeFactory(name =>
            {
                var finalName = pascalSchemaName;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    finalName += ".";
                    finalName += name;
                }

                var result = model.SchemaElements.OfType<EdmComplexType>().FirstOrDefault(x => x.Name == finalName);

                if (result != null)
                {
                    return (result, false);
                }

                result = new EdmComplexType(pascalAppName, finalName);

                model.AddElement(result);

                return (result, true);
            });

            var schemaType = schema.BuildEdmType(withHidden, app.PartitionResolver(), typeFactory);

            var entityType = new EdmEntityType(app.Name.ToPascalCase(), schema.Name);
            entityType.AddStructuralProperty(nameof(IContentEntity.Id).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Created).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.CreatedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModified).ToCamelCase(), EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty(nameof(IContentEntity.LastModifiedBy).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Status).ToCamelCase(), EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty(nameof(IContentEntity.Version).ToCamelCase(), EdmPrimitiveTypeKind.Int32);
            entityType.AddStructuralProperty(nameof(IContentEntity.Data).ToCamelCase(), new EdmComplexTypeReference(schemaType, false));

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("ContentSet", entityType);

            model.AddElement(container);
            model.AddElement(schemaType);
            model.AddElement(entityType);

            return model;
        }

        private static string BuildCacheKey(IAppEntity app, ISchemaEntity schema, bool withHidden)
        {
            return string.Join("_", schema.Id, schema.Version, app.Id, app.Version, withHidden);
        }
    }
}
