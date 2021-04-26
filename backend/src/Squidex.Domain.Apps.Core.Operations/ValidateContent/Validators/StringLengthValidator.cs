// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class StringLengthValidator : IValidator
    {
        private readonly int? minLength;
        private readonly int? maxLength;

        public StringLengthValidator(int? minLength = null, int? maxLength = null)
        {
            if (minLength > maxLength)
            {
                throw new ArgumentException("Min length must be greater than max length.", nameof(minLength));
            }

            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (minLength != null && maxLength != null)
                {
                    if (minLength == maxLength && minLength != stringValue.Length)
                    {
                        addError(context.Path, T.Get("contents.validation.characterCount", new { count = minLength }));
                    }
                    else if (stringValue.Length < minLength || stringValue.Length > maxLength)
                    {
                        addError(context.Path, T.Get("contents.validation.charactersBetween", new { min = minLength, max = maxLength }));
                    }
                }
                else
                {
                    if (minLength != null && stringValue.Length < minLength)
                    {
                        addError(context.Path, T.Get("contents.validation.minLength", new { min = minLength }));
                    }

                    if (maxLength != null && stringValue.Length > maxLength)
                    {
                        addError(context.Path, T.Get("contents.validation.maxLength", new { max = maxLength }));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}