﻿// ==========================================================================
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
                    ["id"] = Builder.GuidProperty($"The id of the {schemaName} content.", true),
                    ["data"] = Builder.ObjectProperty(dataSchema, $"The data of the {schemaName}.", true),
                    ["dataDraft"] = Builder.ObjectProperty(dataSchema, $"The draft data of the {schemaName}.", false),
                    ["version"] = Builder.NumberProperty($"The version of the {schemaName}.", true),
                    ["created"] = Builder.DateTimeProperty($"The date and time when the {schemaName} content has been created.", true),
                    ["createdBy"] = Builder.StringProperty($"The user that has created the {schemaName} content.", true),
                    ["lastModified"] = Builder.DateTimeProperty($"The date and time when the {schemaName} content has been modified last.", true),
                    ["lastModifiedBy"] = Builder.StringProperty($"The user that has updated the {schemaName} content last.", true),
                    ["status"] = Builder.StringProperty($"The status of the content.", true)
                },
                Type = JsonObjectType.Object
            };

            return contentSchema;
        }
    }
}
