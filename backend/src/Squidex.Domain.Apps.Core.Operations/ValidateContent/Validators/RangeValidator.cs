// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class RangeValidator<TValue> : IValidator where TValue : struct, IComparable<TValue>
    {
        private readonly TValue? min;
        private readonly TValue? max;

        public RangeValidator(TValue? min, TValue? max)
        {
            if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
            {
                throw new ArgumentException("Min value must be greater than max value.", nameof(min));
            }

            this.min = min;
            this.max = max;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value != null && value is TValue typedValue)
            {
                if (min.HasValue && max.HasValue)
                {
                    if (Equals(min, max) && Equals(min.Value, max.Value))
                    {
                        addError(context.Path, T.Get("contents.validation.exactValue", new { value = max.Value }));
                    }
                    else if (typedValue.CompareTo(min.Value) < 0 || typedValue.CompareTo(max.Value) > 0)
                    {
                        addError(context.Path, T.Get("contents.validation.between", new { min, max }));
                    }
                }
                else
                {
                    if (min.HasValue && typedValue.CompareTo(min.Value) < 0)
                    {
                        addError(context.Path, T.Get("contents.validation.min", new { min }));
                    }

                    if (max.HasValue && typedValue.CompareTo(max.Value) > 0)
                    {
                        addError(context.Path, T.Get("contents.validation.max", new { max }));
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
