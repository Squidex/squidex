// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NJsonSchema;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class PropertyPathValidator
    {
        public static bool TryGetProperty(this PropertyPath path, JsonSchema schema, List<string> errors, [MaybeNullWhen(false)] out JsonSchema property)
        {
            foreach (var element in path)
            {
                var parent = schema.Reference ?? schema;

                if (parent.Properties.TryGetValue(element, out var p))
                {
                    schema = p;

                    if (schema.Type == JsonObjectType.None && schema.Reference == null)
                    {
                        break;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(parent.Title))
                    {
                        errors.Add($"'{element}' is not a property of '{parent.Title}'.");
                    }
                    else
                    {
                        errors.Add($"Path '{path}' does not point to a valid property in the model.");
                    }

                    property = null!;

                    return false;
                }
            }

            property = schema;

            return true;
        }
    }
}
