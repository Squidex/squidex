// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class StringLengthValidator : IValidator
    {
        private readonly int? minLength;
        private readonly int? maxLength;

        public StringLengthValidator(int? minLength, int? maxLength)
        {
            if (minLength.HasValue && maxLength.HasValue && minLength.Value >= maxLength.Value)
            {
                throw new ArgumentException("Min length must be greater than max length.", nameof(minLength));
            }

            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (minLength.HasValue && stringValue.Length < minLength.Value)
                {
                    addError(context.Path, $"Must have more than '{minLength}' characters.");
                }

                if (maxLength.HasValue && stringValue.Length > maxLength.Value)
                {
                    addError(context.Path, $"Must have less than '{maxLength}' characters.");
                }
            }

            return TaskHelper.Done;
        }
    }
}
