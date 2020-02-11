// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public sealed class ContentSchemaBuilder
    {
        public JsonSchema CreateContentSchema(Schema schema, JsonSchema dataSchema)
        {
            Guard.NotNull(schema);
            Guard.NotNull(dataSchema);

            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            var contentSchema = new JsonSchema
            {
                Properties =
                {
                    ["id"] = SchemaBuilder.GuidProperty($"The id of the {schemaName} content.", true),
                    ["data"] = SchemaBuilder.ObjectProperty(dataSchema, $"The data of the {schemaName}.", true),
                    ["dataDraft"] = SchemaBuilder.ObjectProperty(dataSchema, $"The draft data of the {schemaName}."),
                    ["version"] = SchemaBuilder.NumberProperty($"The version of the {schemaName}.", true),
                    ["created"] = SchemaBuilder.DateTimeProperty($"The date and time when the {schemaName} content has been created.", true),
                    ["createdBy"] = SchemaBuilder.StringProperty($"The user that has created the {schemaName} content.", true),
                    ["lastModified"] = SchemaBuilder.DateTimeProperty($"The date and time when the {schemaName} content has been modified last.", true),
                    ["lastModifiedBy"] = SchemaBuilder.StringProperty($"The user that has updated the {schemaName} content last.", true),
                    ["status"] = SchemaBuilder.StringProperty($"The status of the content.", true)
                },
                Type = JsonObjectType.Object
            };

            return contentSchema;
        }
    }
}
