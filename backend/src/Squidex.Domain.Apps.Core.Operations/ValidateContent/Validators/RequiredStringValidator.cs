// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public class RequiredStringValidator(bool validateEmptyStrings = false) : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        if (context.IsOptional)
        {
            return;
        }

        if (value.IsNullOrUndefined() || IsEmptyString(value))
        {
            context.AddError(T.Get("contents.validation.required"));
        }
    }

    private bool IsEmptyString(object? value)
    {
        return value is string typed && validateEmptyStrings && string.IsNullOrWhiteSpace(typed);
    }
}
