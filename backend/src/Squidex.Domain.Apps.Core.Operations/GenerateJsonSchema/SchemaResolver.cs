// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.GenerateJsonSchema
{
    public class SchemaResolver
    {
        public static readonly SchemaResolver Default = new SchemaResolver();

        public virtual bool ProvidesComponents => false;

        protected SchemaResolver()
        {
        }

        public virtual JsonSchema Register(JsonSchema schema, string typeName)
        {
            return schema;
        }

        public virtual (string?, JsonSchema?) GetComponent(Schema schema)
        {
            return default;
        }
    }
}
