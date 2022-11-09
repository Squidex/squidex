// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters;

public static class ContentQueryModel
{
    public static QueryModel Build(Schema? schema, PartitionResolver partitionResolver, ResolvedComponents components)
    {
        var fields = new List<FilterField>
        {
            new FilterField(FilterSchema.String, "id")
            {
                Description = FieldDescriptions.EntityId
            },
            new FilterField(FilterSchema.Boolean, "isDeleted")
            {
                Description = FieldDescriptions.EntityIsDeleted
            },
            new FilterField(FilterSchema.DateTime, "created")
            {
                Description = FieldDescriptions.EntityCreated
            },
            new FilterField(SharedSchemas.User, "createdBy")
            {
                Description = FieldDescriptions.EntityCreatedBy
            },
            new FilterField(FilterSchema.DateTime, "lastModified")
            {
                Description = FieldDescriptions.EntityLastModified
            },
            new FilterField(SharedSchemas.User, "lastModifiedBy")
            {
                Description = FieldDescriptions.EntityLastModifiedBy
            },
            new FilterField(FilterSchema.Number, "version")
            {
                Description = FieldDescriptions.EntityVersion
            },
            new FilterField(SharedSchemas.Status, "status")
            {
                Description = FieldDescriptions.ContentStatus
            },
            new FilterField(SharedSchemas.Status, "newStatus", IsNullable: true)
            {
                Description = FieldDescriptions.ContentNewStatus
            }
        };

        var translationStatusSchema = BuildTranslationStatus(partitionResolver);

        fields.Add(new FilterField(translationStatusSchema, "translationStatus"));

        if (schema != null)
        {
            var dataSchema = schema.BuildDataSchema(partitionResolver, components);

            fields.Add(new FilterField(dataSchema, "data")
            {
                Description = FieldDescriptions.ContentData
            });
        }

        var filterSchema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = fields.ToReadonlyList()
        };

        return new QueryModel { Schema = filterSchema };
    }

    private static FilterSchema BuildTranslationStatus(PartitionResolver partitionResolver)
    {
        var fields = new List<FilterField>();

        foreach (var key in partitionResolver(Partitioning.Language).AllKeys)
        {
            fields.Add(new FilterField(FilterSchema.Number, key)
            {
                Description = string.Format(CultureInfo.InvariantCulture, FieldDescriptions.TranslationStatusLanguage, key)
            });
        }

        return new FilterSchema(FilterSchemaType.Object)
        {
            Fields = fields.ToReadonlyList()
        };
    }
}
