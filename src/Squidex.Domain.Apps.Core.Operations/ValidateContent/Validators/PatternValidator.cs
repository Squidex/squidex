// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class PatternValidator : IValidator
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(20);
        private readonly Regex regex;
        private readonly string errorMessage;

        public PatternValidator(string pattern, string errorMessage = null)
        {
            this.errorMessage = errorMessage;

            regex = new Regex("^" + pattern + "$", RegexOptions.None, Timeout);
        }

        public Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value is string stringValue)
            {
                if (!string.IsNullOrEmpty(stringValue))
                {
                    try
                    {
                        if (!regex.IsMatch(stringValue))
                        {
                            if (string.IsNullOrWhiteSpace(errorMessage))
                            {
                                addError(context.Path, "Not valid.");
                            }
                            else
                            {
                                addError(context.Path, errorMessage);
                            }
                        }
                    }
                    catch
                    {
                        addError(context.Path, "Regex is too slow.");
                    }
                }
            }

            return TaskHelper.Done;
        }
    }
}
