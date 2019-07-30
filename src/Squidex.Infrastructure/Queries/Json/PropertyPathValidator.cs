// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NJsonSchema;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class PropertyPathValidator
    {
        public static bool TryGetProperty(this PropertyPath path, JsonSchema schema, List<string> errors, out JsonSchema property)
        {
            foreach (var element in path)
            {
                if (schema.Properties.TryGetValue(element, out var p))
                {
                    schema = p;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(schema.Title))
                    {
                        errors.Add($"'{element}' is not a property of '{schema.Title}'.");
                    }
                    else
                    {
                        errors.Add($"Path '{path}' does not point to a valid property in the model.");
                    }

                    property = null;

                    return false;
                }
            }

            property = schema;

            return true;
        }
    }
}
