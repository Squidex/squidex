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
    public class RequiredValidator : IValidator
    {
        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value == null && !context.IsOptional)
            {
                addError("<FIELD> is required.");
            }

            return TaskHelper.Done;
        }
    }
}
