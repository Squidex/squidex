// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Core.GenerateFilters
{
    public static class AssetQueryModel
    {
        public static QueryModel Build()
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
                new FilterableField(FilterableFieldType.Boolean, "isProtected")
                {
                    Description = FieldDescriptions.AssetIsProtected
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
                new FilterableField(FilterableFieldType.String, "fileName")
                {
                    Description = FieldDescriptions.AssetFileName
                },
                new FilterableField(FilterableFieldType.String, "fileHash")
                {
                    Description = FieldDescriptions.AssetFileHash
                },
                new FilterableField(FilterableFieldType.Number, "fileSize")
                {
                    Description = FieldDescriptions.AssetFileSize
                },
                new FilterableField(FilterableFieldType.Number, "fileSize")
                {
                    Description = FieldDescriptions.AssetFileSize
                },
                new FilterableField(FilterableFieldType.Number, "fileVersion")
                {
                    Description = FieldDescriptions.AssetFileVersion
                },
                new FilterableField(FilterableFieldType.Number, "fileVersion")
                {
                    Description = FieldDescriptions.AssetFileVersion
                },
                new FilterableField(FilterableFieldType.Any, "metadata")
                {
                    Description = FieldDescriptions.AssetMetadata
                },
                new FilterableField(FilterableFieldType.String, "mimeType")
                {
                    Description = FieldDescriptions.AssetMimeType
                },
                new FilterableField(FilterableFieldType.String, "slug")
                {
                    Description = FieldDescriptions.AssetSlug
                },
                new FilterableField(FilterableFieldType.String, "tags")
                {
                    Description = FieldDescriptions.AssetTags
                },
                new FilterableField(FilterableFieldType.String, "type")
                {
                    Description = FieldDescriptions.AssetType
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

            return new QueryModel
            {
                Fields = fields.ToReadonlyList()
            };
        }
    }
}
