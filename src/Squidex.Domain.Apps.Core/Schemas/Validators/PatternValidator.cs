// ==========================================================================
//  PatternValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public class PatternValidator : IValidator
    {
        private readonly Regex regex;
        private readonly string errorMessage;

        public PatternValidator(string pattern, string errorMessage = null)
        {
            this.errorMessage = errorMessage;

            regex = new Regex("^" + pattern + "$");
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value is string stringValue)
            {
                if (!string.IsNullOrEmpty(stringValue) && !regex.IsMatch(stringValue))
                {
                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        addError("<FIELD> is not valid");
                    }
                    else
                    {
                        addError(errorMessage);
                    }
                }
            }

            return TaskHelper.Done;
        }
    }
}
