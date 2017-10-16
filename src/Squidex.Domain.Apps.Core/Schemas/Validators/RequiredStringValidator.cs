// ==========================================================================
//  RequiredStringValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public class RequiredStringValidator : IValidator
    {
        private readonly bool validateEmptyStrings;

        public RequiredStringValidator(bool validateEmptyStrings = false)
        {
            this.validateEmptyStrings = validateEmptyStrings;
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (context.IsOptional || (value != null && !(value is string)))
            {
                return TaskHelper.Done;
            }

            var valueAsString = (string)value;

            if (valueAsString == null || (validateEmptyStrings && string.IsNullOrWhiteSpace(valueAsString)))
            {
                addError("<FIELD> is required.");
            }

            return TaskHelper.Done;
        }
    }
}
