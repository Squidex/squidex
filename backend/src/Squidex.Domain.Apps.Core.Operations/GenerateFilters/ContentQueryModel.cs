// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters
{
    public static class ContentQueryModel
    {
        public static QueryModel Build(Schema? schema, PartitionResolver partitionResolver, ResolvedComponents components)
        {
            var fields = new List<FilterableField>
            {
                new FilterableField(FilterableFieldType.String, "id")
                {
                    FieldHints = FieldDescriptions.EntityId
                },
                new FilterableField(FilterableFieldType.Boolean, "isDeleted")
                {
                    FieldHints = FieldDescriptions.EntityIsDeleted
                },
                new FilterableField(FilterableFieldType.DateTime, "created")
                {
                    FieldHints = FieldDescriptions.EntityCreated
                },
                new FilterableField(FilterableFieldType.String, "createdBy")
                {
                    FieldHints = FieldDescriptions.EntityCreatedBy,
                    Extra = new
                    {
                        editor = "User"
                    }
                },
                new FilterableField(FilterableFieldType.DateTime, "lastModified")
                {
                    FieldHints = FieldDescriptions.EntityLastModified
                },
                new FilterableField(FilterableFieldType.String, "lastModifiedBy")
                {
                    FieldHints = FieldDescriptions.EntityLastModifiedBy,
                    Extra = new
                    {
                        editor = "User"
                    }
                },
                new FilterableField(FilterableFieldType.String, "version")
                {
                    FieldHints = FieldDescriptions.EntityVersion
                },
                new FilterableField(FilterableFieldType.String, "status")
                {
                    FieldHints = FieldDescriptions.ContentStatus,
                    Extra = new
                    {
                        editor = "Status"
                    },
                    IsNullable = false
                },
                new FilterableField(FilterableFieldType.String, "newStatus")
                {
                    FieldHints = FieldDescriptions.ContentNewStatus,
                    Extra = new
                    {
                        editor = "Status"
                    },
                    IsNullable = true
                }
            };

            if (schema != null)
            {
                fields.Add(schema.BuildDataField(partitionResolver, components));
            }

            return new QueryModel
            {
                Fields = fields.ToReadonlyList()
            };
        }
    }
}
