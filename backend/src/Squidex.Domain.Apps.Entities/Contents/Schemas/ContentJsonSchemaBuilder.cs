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
        public static JsonSchema BuildSchema(string name, JsonSchema? dataSchema, bool extended = false, bool withDeleted = false)
        {
            var jsonSchema = new JsonSchema
            {
                Properties =
                {
                    ["id"] = SchemaBuilder.StringProperty($"The id of the {name} content.", true),
                    ["created"] = SchemaBuilder.DateTimeProperty($"The date and time when the {name} content has been created.", true),
                    ["createdBy"] = SchemaBuilder.StringProperty($"The user that has created the {name} content.", true),
                    ["lastModified"] = SchemaBuilder.DateTimeProperty($"The date and time when the {name} content has been modified last.", true),
                    ["lastModifiedBy"] = SchemaBuilder.StringProperty($"The user that has updated the {name} content last.", true),
                    ["newStatus"] = SchemaBuilder.StringProperty("The new status of the content."),
                    ["status"] = SchemaBuilder.StringProperty("The status of the content.", true),
                },
                Type = JsonObjectType.Object
            };

            if (withDeleted)
            {
                jsonSchema.Properties["isDeleted"] = SchemaBuilder.BooleanProperty("True when deleted.", false);
            }

            if (extended)
            {
                jsonSchema.Properties["newStatusColor"] = SchemaBuilder.StringProperty("The color of the new status.", false);
                jsonSchema.Properties["schema"] = SchemaBuilder.StringProperty("The name of the schema.", true);
                jsonSchema.Properties["SchemaName"] = SchemaBuilder.StringProperty("The display name of the schema.", true);
                jsonSchema.Properties["statusColor"] = SchemaBuilder.StringProperty("The color of the status.", true);
            }

            if (dataSchema != null)
            {
                jsonSchema.Properties["data"] = SchemaBuilder.ReferenceProperty(dataSchema, $"The data of the {name}.", true);
            }

            return jsonSchema;
        }
    }
}
