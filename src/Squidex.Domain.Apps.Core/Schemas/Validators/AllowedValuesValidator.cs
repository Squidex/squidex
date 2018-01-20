// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class AllowedValuesValidator<T> : IValidator
    {
        private readonly T[] allowedValues;

        public AllowedValuesValidator(params T[] allowedValues)
        {
            Guard.NotNull(allowedValues, nameof(allowedValues));

            this.allowedValues = allowedValues;
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value == null)
            {
                return TaskHelper.Done;
            }

            var typedValue = (T)value;

            if (!allowedValues.Contains(typedValue))
            {
                addError("<FIELD> is not an allowed value.");
            }

            return TaskHelper.Done;
        }
    }
}