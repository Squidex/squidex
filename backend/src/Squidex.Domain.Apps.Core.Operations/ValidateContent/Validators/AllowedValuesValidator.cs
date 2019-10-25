// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class AllowedValuesValidator<T> : IValidator
    {
        private readonly IEnumerable<T> allowedValues;

        public AllowedValuesValidator(params T[] allowedValues)
            : this((IEnumerable<T>)allowedValues)
        {
        }

        public AllowedValuesValidator(IEnumerable<T> allowedValues)
        {
            Guard.NotNull(allowedValues);

            this.allowedValues = allowedValues;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value != null && value is T typedValue && !allowedValues.Contains(typedValue))
            {
                addError(context.Path, "Not an allowed value.");
            }

            return TaskHelper.Done;
        }
    }
}