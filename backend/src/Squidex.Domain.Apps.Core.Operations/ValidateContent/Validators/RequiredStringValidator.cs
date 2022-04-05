// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class RequiredStringValidator : IValidator
    {
        private readonly bool validateEmptyStrings;

        public RequiredStringValidator(bool validateEmptyStrings = false)
        {
            this.validateEmptyStrings = validateEmptyStrings;
        }

        public ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.IsOptional)
            {
                return default;
            }

            if (value.IsNullOrUndefined() || IsEmptyString(value))
            {
                addError(context.Path, T.Get("contents.validation.required"));
            }

            return default;
        }

        private bool IsEmptyString(object? value)
        {
            return value is string typed && validateEmptyStrings && string.IsNullOrWhiteSpace(typed);
        }
    }
}
