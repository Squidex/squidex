// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public class RequiredValidator : IValidator
{
    public void Validate(object? value, ValidationContext context)
    {
        if (value.IsNullOrUndefined() && !context.IsOptional)
        {
            context.AddError(context.Path, T.Get("contents.validation.required"));
        }
    }
}
