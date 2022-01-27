// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters
{
    public delegate (JsonSchema Reference, JsonSchema? Actual) JsonTypeFactory(string name);

    public static class JsonSchemaExtensions
    {
        public static FilterableField BuildDataField(this Schema schema, PartitionResolver partitionResolver,
            ResolvedComponents components)
        {
            Guard.NotNull(partitionResolver, nameof(partitionResolver));
            Guard.NotNull(components, nameof(components));

            var fields = new List<FilterableField>();

            foreach (var field in schema.Fields.ForApi(true))
            {
                var filterableField = FilterVisitor.BuildProperty(field, components);

                if (filterableField == null)
                {
                    continue;
                }

                var partitioning = partitionResolver(field.Partitioning);

                var nestedFields = new List<FilterableField>();

                foreach (var partitionKey in partitioning.AllKeys)
                {
                }

                fields.Add(new FilterableField(FilterableFieldType.Object, field.Name)
                {
                    Fields = nestedFields.ToReadonlyList()
                });
            }

            return new FilterableField(FilterableFieldType.Object, "data")
            {
                Fields = fields.ToReadonlyList()
            };
        }

        public static JsonSchemaProperty SetDescription(this JsonSchemaProperty jsonProperty, IField field)
        {
            if (!string.IsNullOrWhiteSpace(field.RawProperties.Hints))
            {
                jsonProperty.Description = $"{field.Name} ({field.RawProperties.Hints})";
            }
            else
            {
                jsonProperty.Description = field.Name;
            }

            return jsonProperty;
        }
    }
}
