// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class AggregateValidator(IEnumerable<IValidator>? validators) : IValidator
{
    private readonly IValidator[]? validators = validators?.ToArray();

    public void Validate(object? value, ValidationContext context)
    {
        if (validators is not { Length: > 0 })
        {
            return;
        }

        try
        {
            foreach (var validator in validators)
            {
                validator.Validate(value, context);
            }
        }
        catch
        {
            context.AddError(T.Get("contents.validation.error"));
        }
    }
}
