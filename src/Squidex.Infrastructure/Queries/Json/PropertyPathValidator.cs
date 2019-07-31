// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class PropertyPathValidator
    {
        public static bool TryGetProperty(this PropertyPath path, JsonSchema schema, out JsonSchema property)
        {
            foreach (var element in path)
            {
                if (schema.Properties.TryGetValue(element, out var p))
                {
                    schema = p;
                }
                else
                {
                    property = null;

                    return false;
                }
            }

            property = schema;

            return true;
        }
    }
}
