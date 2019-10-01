// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class RequiredStringValidator : IValidator
    {
        private readonly bool validateEmptyStrings;

        public RequiredStringValidator(bool validateEmptyStrings = false)
        {
            this.validateEmptyStrings = validateEmptyStrings;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (context.IsOptional)
            {
                return TaskHelper.Done;
            }

            if (value.IsNullOrUndefined() || IsEmptyString(value))
            {
                addError(context.Path, "Field is required.");
            }

            return TaskHelper.Done;
        }

        private bool IsEmptyString(object? value)
        {
            return value is string typed && validateEmptyStrings && string.IsNullOrWhiteSpace(typed);
        }
    }
}
