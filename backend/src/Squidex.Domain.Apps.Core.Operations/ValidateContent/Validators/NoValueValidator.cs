// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class NoValueValidator : IValidator
    {
        public static readonly NoValueValidator Instance = new NoValueValidator();

        private NoValueValidator()
        {
        }

        public ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (!value.IsUndefined())
            {
                addError(context.Path, T.Get("contents.validation.mustBeEmpty"));
            }

            return default;
        }
    }
}
