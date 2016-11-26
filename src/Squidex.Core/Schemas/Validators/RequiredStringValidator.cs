// ==========================================================================
//  RequiredStringValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public class RequiredStringValidator : IValidator
    {
        private readonly bool validateEmptyStrings;

        public RequiredStringValidator(bool validateEmptyStrings = false)
        {
            this.validateEmptyStrings = validateEmptyStrings;
        }

        public Task ValidateAsync(object value, ICollection<string> errors)
        {
            if (value != null && !(value is string))
            {
                return TaskHelper.Done;
            }

            var valueAsString = (string) value;

            if (valueAsString == null || (validateEmptyStrings && string.IsNullOrWhiteSpace(valueAsString)))
            {
                errors.Add("<FIELD> is required");
            }

            return TaskHelper.Done;
        }
    }
}
