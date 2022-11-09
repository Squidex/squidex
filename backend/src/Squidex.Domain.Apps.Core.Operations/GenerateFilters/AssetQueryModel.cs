// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters;

public static class AssetQueryModel
{
    public static QueryModel Build()
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
            new FilterField(FilterSchema.String, "status")
            {
                Description = FieldDescriptions.ContentStatus
            },
            new FilterField(FilterSchema.String, "version")
            {
                Description = FieldDescriptions.EntityVersion
            },
            new FilterField(FilterSchema.String, "fileHash")
            {
                Description = FieldDescriptions.AssetFileHash
            },
            new FilterField(FilterSchema.String, "fileName")
            {
                Description = FieldDescriptions.AssetFileName
            },
            new FilterField(FilterSchema.Number, "fileSize")
            {
                Description = FieldDescriptions.AssetFileSize
            },
            new FilterField(FilterSchema.Number, "fileVersion")
            {
                Description = FieldDescriptions.AssetFileVersion
            },
            new FilterField(FilterSchema.Boolean, "isProtected")
            {
                Description = FieldDescriptions.AssetIsProtected
            },
            new FilterField(FilterSchema.Any, "metadata")
            {
                Description = FieldDescriptions.AssetMetadata
            },
            new FilterField(FilterSchema.String, "mimeType")
            {
                Description = FieldDescriptions.AssetMimeType
            },
            new FilterField(FilterSchema.String, "slug")
            {
                Description = FieldDescriptions.AssetSlug
            },
            new FilterField(FilterSchema.StringArray, "tags")
            {
                Description = FieldDescriptions.AssetTags
            },
            new FilterField(FilterSchema.String, "type")
            {
                Description = FieldDescriptions.AssetType
            }
        };

        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = fields.ToReadonlyList()
        };

        return new QueryModel { Schema = schema };
    }
}
