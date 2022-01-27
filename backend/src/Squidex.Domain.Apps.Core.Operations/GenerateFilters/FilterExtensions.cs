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

namespace Squidex.Domain.Apps.Core.GenerateFilters
{
    public static class JsonSchemaExtensions
    {
        public static FilterableField BuildDataField(this Schema schema, PartitionResolver partitionResolver,
            ResolvedComponents components)
        {
            Guard.NotNull(partitionResolver, nameof(partitionResolver));
            Guard.NotNull(components, nameof(components));

            var fields = new List<FilterableField>();

            var schemaName = schema.DisplayName();

            foreach (var field in schema.Fields.ForApi(true))
            {
                var filterableField = FilterVisitor.BuildProperty(field, components);

                if (filterableField == null)
                {
                    continue;
                }

                var partitioning = partitionResolver(field.Partitioning);
                var partitionFields = new List<FilterableField>();

                foreach (var partitionKey in partitioning.AllKeys)
                {
                    var isNullable = partitioning.IsOptional(partitionKey) || !field.RawProperties.IsRequired;

                    var partitionField = filterableField with
                    {
                        IsNullable = isNullable,
                        FieldHints = FieldPartitionDescription(field, partitioning, partitionKey),
                        Fields = filterableField.Fields
                    };

                    partitionFields.Add(partitionField);
                }

                var fieldGroup = new FilterableField(FilterableFieldType.Object, field.Name)
                {
                    FieldHints = FieldDescription(schemaName, field),
                    Fields = partitionFields.ToReadonlyList()
                };

                fields.Add(fieldGroup);
            }

            var dataField = new FilterableField(FilterableFieldType.Object, "data")
            {
                FieldHints = FieldDescriptions.ContentData,
                Fields = fields.ToReadonlyList()
            };

            return dataField;
        }

        private static string FieldPartitionDescription(RootField field, IFieldPartitioning partitioning, string partitionKey)
        {
            var name = field.DisplayName();

            return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ContentPartitionField, name, partitioning.GetName(partitionKey));
        }

        private static string FieldDescription(string schemaName, RootField field)
        {
            var name = field.DisplayName();

            return string.Format(CultureInfo.InvariantCulture, FieldDescriptions.ContentField, name, schemaName);
        }
    }
}
