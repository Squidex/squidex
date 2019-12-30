// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class SchemaExtensions
    {
        public static bool IsDynamic(this JsonSchema schema)
        {
            return schema.Type == JsonObjectType.Object && schema.Properties.Count == 0 && schema.AllowAdditionalProperties == true;
        }
    }
}
