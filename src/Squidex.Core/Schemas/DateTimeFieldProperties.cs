// ==========================================================================
//  DateTimeFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("DateTime")]
    public sealed class DateTimeFieldProperties : FieldProperties
    {
        private DateTimeFieldEditor editor;
        private DateTimeOffset? maxValue;
        private DateTimeOffset? minValue;
        private DateTimeOffset? defaultValue;

        public DateTimeOffset? MaxValue
        {
            get { return maxValue; }
            set
            {
                ThrowIfFrozen();

                maxValue = value;
            }
        }

        public DateTimeOffset? MinValue
        {
            get { return minValue; }
            set
            {
                ThrowIfFrozen();

                minValue = value;
            }
        }

        public DateTimeOffset? DefaultValue
        {
            get { return defaultValue; }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public DateTimeFieldEditor Editor
        {
            get { return editor; }
            set
            {
                ThrowIfFrozen();

                editor = value;
            }
        }

        public override JToken GetDefaultValue()
        {
            return DefaultValue;
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (!Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor ist not a valid value", nameof(Editor));
            }

            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value >= MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value", nameof(MinValue), nameof(MaxValue));
            }

            if (DefaultValue.HasValue && MinValue.HasValue && DefaultValue.Value < MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value", nameof(DefaultValue));
            }

            if (DefaultValue.HasValue && MaxValue.HasValue && DefaultValue.Value > MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value", nameof(DefaultValue));
            }
        }
    }
}
