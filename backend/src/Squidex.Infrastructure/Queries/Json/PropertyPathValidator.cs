// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class PropertyPathValidator
    {
        public static bool TryGetField(this PropertyPath path, QueryModel model, List<string> errors, [MaybeNullWhen(false)] out FilterableField field)
        {
            field = null!;

            var list = model.Fields;
            var index = 0;

            foreach (var element in path)
            {
                var current = list.FirstOrDefault(x => x.Path == element);

                if (current == null)
                {
                    break;
                }

                if (current.Fields == null || current.Fields.Count == 0 || element == path[^1])
                {
                    field = current;
                    return true;
                }

                if (current.Fields != null)
                {
                    list = current.Fields;
                }
                else
                {
                    break;
                }

                index++;
            }

            if (index > 0)
            {
                errors.Add($"'{path[index]}' is not a property of '{string.Join('.', path.Take(index))}'.");
            }
            else
            {
                errors.Add($"Path '{path}' does not point to a valid property in the model.");
            }

            return false;
        }
    }
}
