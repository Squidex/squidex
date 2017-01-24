// ==========================================================================
//  SchemaExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Read.MongoDb.Contents.Visitors
{
    public static class SchemaExtensions
    {
        public static EdmModel BuildEdmModel(this Schema schema, HashSet<Language> languages)
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
            entityType.AddStructuralProperty("created", EdmPrimitiveTypeKind.Date);
            entityType.AddStructuralProperty("createdBy", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("lastModified", EdmPrimitiveTypeKind.Date);
            entityType.AddStructuralProperty("lastModifiedBy", EdmPrimitiveTypeKind.String);

            model.AddElement(container);
            model.AddElement(schemaType);
            model.AddElement(entityType);

            container.AddEntitySet($"{schema.Name}_Set", entityType);

            return model;
        }

        public static ODataUriParser ParseQuery(this Schema schema, HashSet<Language> languages, string query)
        {
            var model = schema.BuildEdmModel(languages);

            var parser = new ODataUriParser(model, new Uri($"{schema.Name}_Set?{query}", UriKind.Relative));

            return parser;
        }
    }
}
