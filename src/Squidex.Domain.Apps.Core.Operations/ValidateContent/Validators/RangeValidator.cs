﻿// ==========================================================================
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
    public sealed class RangeValidator<T> : IValidator where T : struct, IComparable<T>
    {
        private readonly T? min;
        private readonly T? max;

        public RangeValidator(T? min, T? max)
        {
            if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) >= 0)
            {
                throw new ArgumentException("Min value must be greater than max value.", nameof(min));
            }

            this.min = min;
            this.max = max;
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value == null)
            {
                return TaskHelper.Done;
            }

            var typedValue = (T)value;

            if (min.HasValue && typedValue.CompareTo(min.Value) < 0)
            {
                addError($"<FIELD> must be greater or equals than '{min}'.");
            }

            if (max.HasValue && typedValue.CompareTo(max.Value) > 0)
            {
                addError($"<FIELD> must be less or equals than '{max}'.");
            }

            return TaskHelper.Done;
        }
    }
}