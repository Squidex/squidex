// ==========================================================================
//  ContentSchemaBuilder.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Contents.JsonSchema
{
    public sealed class ContentSchemaBuilder
    {
        public JsonSchema4 CreateContentSchema(Schema schema, JsonSchema4 dataSchema)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(dataSchema, nameof(dataSchema));

            var schemaName = schema.Properties.Label.WithFallback(schema.Name);

            var contentSchema = new JsonSchema4
            {
                Properties =
                {
                    ["id"] = CreateProperty($"The id of the {schemaName} content."),
                    ["data"] = CreateProperty($"The data of the {schemaName}.", dataSchema),
                    ["version"] = CreateProperty($"The version of the {schemaName}.", JsonObjectType.Number),
                    ["created"] = CreateProperty($"The date and time when the {schemaName} content has been created.", "date-time"),
                    ["createdBy"] = CreateProperty($"The user that has created the {schemaName} content."),
                    ["lastModified"] = CreateProperty($"The date and time when the {schemaName} content has been modified last.", "date-time"),
                    ["lastModifiedBy"] = CreateProperty($"The user that has updated the {schemaName} content last.")
                },
                Type = JsonObjectType.Object
            };

            return contentSchema;
        }

        private static JsonProperty CreateProperty(string description, JsonSchema4 dataSchema)
        {
            return new JsonProperty { Description = description, IsRequired = true, Type = JsonObjectType.Object, SchemaReference = dataSchema };
        }

        private static JsonProperty CreateProperty(string description, JsonObjectType type)
        {
            return new JsonProperty { Description = description, IsRequired = true, Type = type };
        }

        private static JsonProperty CreateProperty(string description, string format = null)
        {
            return new JsonProperty { Description = description, Format = format, IsRequired = true, Type = JsonObjectType.String };
        }
    }
}
