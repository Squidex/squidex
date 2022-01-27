// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public static class ContentJsonSchemaBuilder
    {
        public static JsonSchema BuildSchema(JsonSchema? dataSchema, bool extended = false, bool withDeleted = false)
        {
            var jsonSchema = new JsonSchema
            {
                AllowAdditionalProperties = false,
                Properties =
                {
                    ["id"] = JsonTypeBuilder.StringProperty(FieldDescriptions.EntityId, true),
                    ["created"] = JsonTypeBuilder.DateTimeProperty(FieldDescriptions.EntityCreated, true),
                    ["createdBy"] = JsonTypeBuilder.StringProperty(FieldDescriptions.EntityCreatedBy, true, SpecialFormats.User),
                    ["lastModified"] = JsonTypeBuilder.DateTimeProperty(FieldDescriptions.EntityLastModified, true),
                    ["lastModifiedBy"] = JsonTypeBuilder.StringProperty(FieldDescriptions.EntityLastModifiedBy, true, SpecialFormats.User),
                    ["newStatus"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentNewStatus, false, SpecialFormats.Status),
                    ["status"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentStatus, true, SpecialFormats.Status)
                },
                Type = JsonObjectType.Object
            };

            if (withDeleted)
            {
                jsonSchema.Properties["isDeleted"] = JsonTypeBuilder.BooleanProperty(FieldDescriptions.EntityIsDeleted, false);
            }

            if (extended)
            {
                jsonSchema.Properties["newStatusColor"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentNewStatusColor, false);
                jsonSchema.Properties["schema"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentSchema, true);
                jsonSchema.Properties["SchemaName"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentSchemaName, true);
                jsonSchema.Properties["statusColor"] = JsonTypeBuilder.StringProperty(FieldDescriptions.ContentStatusColor, true);
            }

            if (dataSchema != null)
            {
                jsonSchema.Properties["data"] = JsonTypeBuilder.ReferenceProperty(dataSchema, FieldDescriptions.ContentData, true);
            }

            return jsonSchema;
        }
    }
}
