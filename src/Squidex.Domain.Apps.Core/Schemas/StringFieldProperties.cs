// ==========================================================================
//  StringFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(StringField))]
    public sealed class StringFieldProperties : FieldProperties
    {
        private int? minLength;
        private int? maxLength;
        private string pattern;
        private string patternMessage;
        private string defaultValue;
        private ImmutableList<string> allowedValues;
        private StringFieldEditor editor;

        public int? MinLength
        {
            get
            {
                return minLength;
            }
            set
            {
                ThrowIfFrozen();

                minLength = value;
            }
        }

        public int? MaxLength
        {
            get
            {
                return maxLength;
            }
            set
            {
                ThrowIfFrozen();

                maxLength = value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public string Pattern
        {
            get
            {
                return pattern;
            }
            set
            {
                ThrowIfFrozen();

                pattern = value;
            }
        }

        public string PatternMessage
        {
            get
            {
                return patternMessage;
            }
            set
            {
                ThrowIfFrozen();

                patternMessage = value;
            }
        }

        public ImmutableList<string> AllowedValues
        {
            get
            {
                return allowedValues;
            }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }

        public StringFieldEditor Editor
        {
            get
            {
                return editor;
            }
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

        public override bool ShouldApplyDefaultValue(JToken value)
        {
            return value.IsNull() || (value is JValue jValue && Equals(jValue.Value, string.Empty));
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (!Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value", nameof(Editor));
            }

            if ((Editor == StringFieldEditor.Radio || Editor == StringFieldEditor.Dropdown) && (AllowedValues == null || AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values", nameof(AllowedValues));
            }

            if (MaxLength.HasValue && MinLength.HasValue && MinLength.Value >= MaxLength.Value)
            {
                yield return new ValidationError("Max length must be greater than min length", nameof(MinLength), nameof(MaxLength));
            }

            if (Pattern != null && !Pattern.IsValidRegex())
            {
                yield return new ValidationError("Pattern is not a valid expression", nameof(Pattern));
            }

            if (AllowedValues != null && AllowedValues.Count > 0 && (MinLength.HasValue || MaxLength.HasValue))
            {
                yield return new ValidationError("Either allowed values or min and max length can be defined",
                    nameof(AllowedValues),
                    nameof(MinLength),
                    nameof(MaxLength));
            }
        }
    }
}
