// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class UniqueValuesValidator<TValue> : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        if (value is not IEnumerable<TValue> items || !items.Any())
        {
            return;
        }

        var itemsArray = items.ToArray();

        if (itemsArray.Length != itemsArray.Distinct().Count())
        {
            context.AddError(T.Get("contents.validation.duplicates"));
        }
    }
}
