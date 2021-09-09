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
                Properties =
                {
                    ["id"] = SchemaBuilder.StringProperty(FieldDescriptions.EntityId, true),
                    ["created"] = SchemaBuilder.DateTimeProperty(FieldDescriptions.EntityCreated, true),
                    ["createdBy"] = SchemaBuilder.StringProperty(FieldDescriptions.EntityCreatedBy, true),
                    ["lastModified"] = SchemaBuilder.DateTimeProperty(FieldDescriptions.EntityLastModified, true),
                    ["lastModifiedBy"] = SchemaBuilder.StringProperty(FieldDescriptions.EntityLastModifiedBy, true),
                    ["newStatus"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentNewStatus),
                    ["status"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentStatus, true)
                },
                Type = JsonObjectType.Object
            };

            if (withDeleted)
            {
                jsonSchema.Properties["isDeleted"] = SchemaBuilder.BooleanProperty(FieldDescriptions.EntityIsDeleted, false);
            }

            if (extended)
            {
                jsonSchema.Properties["newStatusColor"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentNewStatusColor, false);
                jsonSchema.Properties["schema"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentSchema, true);
                jsonSchema.Properties["SchemaName"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentSchemaName, true);
                jsonSchema.Properties["statusColor"] = SchemaBuilder.StringProperty(FieldDescriptions.ContentStatusColor, true);
            }

            if (dataSchema != null)
            {
                jsonSchema.Properties["data"] = SchemaBuilder.ReferenceProperty(dataSchema, FieldDescriptions.ContentData, true);
            }

            return jsonSchema;
        }
    }
}
