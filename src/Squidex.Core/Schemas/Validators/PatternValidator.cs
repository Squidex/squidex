// ==========================================================================
//  PatternValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Core.Schemas.Validators
{
    public class PatternValidator : IValidator
    {
        private readonly Regex regex;
        private readonly string errorMessage;

        public PatternValidator(string pattern, string errorMessage = null)
        {
            this.errorMessage = errorMessage;

            regex = new Regex(pattern);
        }

        public Task ValidateAsync(object value, ICollection<string> errors)
        {
            var stringValue = value as string;

            if (stringValue == null)
            {
                return TaskHelper.Done;
            }

            if (!regex.IsMatch(stringValue))
            {
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errors.Add("<FIELD> is not valid");
                }
                else
                {
                    errors.Add(errorMessage);
                }
            }

            return TaskHelper.Done;
        }
    }
}
