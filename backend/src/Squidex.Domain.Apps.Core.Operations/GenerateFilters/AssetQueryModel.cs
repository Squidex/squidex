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
                new FilterableField(FilterableFieldType.String, "status")
                {
                    FieldHints = FieldDescriptions.ContentStatus
                },
                new FilterableField(FilterableFieldType.String, "version")
                {
                    FieldHints = FieldDescriptions.EntityVersion
                },
                new FilterableField(FilterableFieldType.String, "fileHash")
                {
                    FieldHints = FieldDescriptions.AssetFileHash
                },
                new FilterableField(FilterableFieldType.String, "fileName")
                {
                    FieldHints = FieldDescriptions.AssetFileName
                },
                new FilterableField(FilterableFieldType.Number, "fileSize")
                {
                    FieldHints = FieldDescriptions.AssetFileSize
                },
                new FilterableField(FilterableFieldType.Number, "fileVersion")
                {
                    FieldHints = FieldDescriptions.AssetFileVersion
                },
                new FilterableField(FilterableFieldType.Boolean, "isProtected")
                {
                    FieldHints = FieldDescriptions.AssetIsProtected
                },
                new FilterableField(FilterableFieldType.Any, "metadata")
                {
                    FieldHints = FieldDescriptions.AssetMetadata
                },
                new FilterableField(FilterableFieldType.String, "mimeType")
                {
                    FieldHints = FieldDescriptions.AssetMimeType
                },
                new FilterableField(FilterableFieldType.String, "slug")
                {
                    FieldHints = FieldDescriptions.AssetSlug
                },
                new FilterableField(FilterableFieldType.StringArray, "tags")
                {
                    FieldHints = FieldDescriptions.AssetTags
                },
                new FilterableField(FilterableFieldType.String, "type")
                {
                    FieldHints = FieldDescriptions.AssetType
                }
            };

            return new QueryModel
            {
                Fields = fields.ToReadonlyList()
            };
        }
    }
}
