// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class UniqueObjectValuesValidator : IValidator
{
    private readonly IEnumerable<string> fields;

    public UniqueObjectValuesValidator(IEnumerable<string> fields)
    {
        this.fields = fields;
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is IEnumerable<JsonObject> objects && objects.Count() > 1)
        {
            Validate(objects, context);
        }
        else if (value is IEnumerable<Component> components && components.Count() > 1)
        {
            Validate(components.Select(x => x.Data), context);
        }
    }

    private void Validate(IEnumerable<JsonObject> items, ValidationContext context)
    {
        var duplicates = new HashSet<JsonValue>(10);

        foreach (var field in fields)
        {
            duplicates.Clear();

            foreach (var item in items)
            {
                if (item.TryGetValue(field, out var fieldValue) && !duplicates.Add(fieldValue))
                {
                    context.AddError(context.Path, T.Get("contents.validation.uniqueObjectValues", new { field }));
                    break;
                }
            }
        }
    }
}
