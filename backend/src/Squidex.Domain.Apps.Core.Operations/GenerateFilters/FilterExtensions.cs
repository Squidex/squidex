// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters;

public static class FilterExtensions
{
    public static FilterSchema BuildDataSchema(this Schema schema, PartitionResolver partitionResolver,
        ResolvedComponents components)
    {
        Guard.NotNull(partitionResolver);
        Guard.NotNull(components);

        var fields = new List<FilterField>();

        var schemaName = schema.DisplayName();

        foreach (var field in schema.Fields.ForApi(true))
        {
            var fieldSchema = FilterVisitor.BuildProperty(field, components);

            if (fieldSchema == null)
            {
                continue;
            }

            var partitioning = partitionResolver(field.Partitioning);
            var partitionFields = new List<FilterField>();

            foreach (var partitionKey in partitioning.AllKeys)
            {
                var partitionDescription = FieldPartitionDescription(field, partitioning.GetName(partitionKey) ?? partitionKey);

                var partitionField = new FilterField(
                    fieldSchema,
                    partitionKey,
                    partitionDescription,
                    true);

                partitionFields.Add(partitionField);
            }

            var filterable = new FilterField(
                new FilterSchema(FilterSchemaType.Object)
                {
                    Fields = partitionFields.ToReadonlyList()
                },
                field.Name,
                FieldDescription(schemaName, field));

            fields.Add(filterable);
        }

        var dataSchema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = fields.ToReadonlyList()
        };

        return dataSchema;
    }

    private static string FieldPartitionDescription(RootField field, string partition)
    {
        var name = field.DisplayName();

        return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ContentPartitionField, name, partition);
    }

    private static string FieldDescription(string schemaName, RootField field)
    {
        var name = field.DisplayName();

        return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ContentField, name, schemaName);
    }
}
