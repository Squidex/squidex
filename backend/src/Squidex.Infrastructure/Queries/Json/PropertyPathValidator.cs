// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries.Json;

public static class PropertyPathValidator
{
    public static IEnumerable<FilterField> GetMatchingFields(this PropertyPath path, FilterSchema schema, List<string> errors)
    {
        var lastIndex = path.Count - 1;

        List<FilterField>? result = null;

        void Check(int index, FilterSchema schema)
        {
            if (schema.Fields == null)
            {
                return;
            }

            var fields = schema.Fields.Where(x => x.Path == path[index]);

            foreach (var field in fields)
            {
                if (index == lastIndex || field.Schema.Type == FilterSchemaType.Any)
                {
                    result ??= new List<FilterField>();
                    result.Add(field);
                }
                else
                {
                    Check(index + 1, field.Schema);
                }
            }
        }

        Check(0, schema);

        if (result == null)
        {
            errors.Add(Errors.InvalidPath(path.ToString()));
        }

        return result as IEnumerable<FilterField> ?? Array.Empty<FilterField>();
    }
}
