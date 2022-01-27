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
        public static QueryModel BuildModel(Schema? schema, PartitionResolver partitionResolver, ResolvedComponents components)
        {
            var fields = new List<FilterableField>
            {
                new FilterableField(FilterableFieldType.String, "id")
                {
                    Description = FieldDescriptions.EntityId
                },
                new FilterableField(FilterableFieldType.Boolean, "isDeleted")
                {
                    Description = FieldDescriptions.EntityIsDeleted
                },
                new FilterableField(FilterableFieldType.DateTime, "created")
                {
                    Description = FieldDescriptions.EntityCreated
                },
                new FilterableField(FilterableFieldType.DateTime, "lastModified")
                {
                    Description = FieldDescriptions.EntityCreated
                },
                new FilterableField(FilterableFieldType.String, "status")
                {
                    Description = FieldDescriptions.ContentStatus
                },
                new FilterableField(FilterableFieldType.String, "version")
                {
                    Description = FieldDescriptions.EntityVersion
                },
                new FilterableField(FilterableFieldType.String, "newStatus")
                {
                    Description = FieldDescriptions.ContentNewStatus,
                    IsNullable = true
                },
                new FilterableField(FilterableFieldType.String, "createdBy")
                {
                    Description = FieldDescriptions.EntityCreated,
                    Extra = "user"
                },
                new FilterableField(FilterableFieldType.String, "lastModifiedBy")
                {
                    Description = FieldDescriptions.EntityCreated,
                    Extra = "user"
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
