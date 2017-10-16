// ==========================================================================
//  FieldPropertiesValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public sealed class FieldPropertiesValidator : IFieldPropertiesVisitor<IEnumerable<ValidationError>>
    {
        private static readonly FieldPropertiesValidator Instance = new FieldPropertiesValidator();

        private FieldPropertiesValidator()
        {
        }

        public static IEnumerable<ValidationError> Validate(FieldProperties properties)
        {
            return properties.Accept(Instance);
        }

        public IEnumerable<ValidationError> Visit(AssetsFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value >= properties.MaxItems.Value)
            {
                yield return new ValidationError("Max items must be greater than min items.",
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(BooleanFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value.",
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(DateTimeFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value.",
                    nameof(properties.Editor));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value.",
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value.",
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value.",
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.CalculatedDefaultValue.HasValue)
            {
                if (!properties.CalculatedDefaultValue.Value.IsEnumValue())
                {
                    yield return new ValidationError("Calculated default value is not valid.",
                        nameof(properties.CalculatedDefaultValue));
                }

                if (properties.DefaultValue.HasValue)
                {
                    yield return new ValidationError("Calculated default value and default value cannot be used together.",
                        nameof(properties.CalculatedDefaultValue),
                        nameof(properties.DefaultValue));
                }
            }
        }

        public IEnumerable<ValidationError> Visit(GeolocationFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value.",
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(JsonFieldProperties properties)
        {
            yield break;
        }

        public IEnumerable<ValidationError> Visit(NumberFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value.",
                    nameof(properties.Editor));
            }

            if ((properties.Editor == NumberFieldEditor.Radio || properties.Editor == NumberFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values.",
                    nameof(properties.AllowedValues));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value.",
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value.",
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value.",
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.AllowedValues != null && properties.AllowedValues.Count > 0 && (properties.MinValue.HasValue || properties.MaxValue.HasValue))
            {
                yield return new ValidationError("Either allowed values or min and max value can be defined.",
                    nameof(properties.AllowedValues),
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }
        }

        public IEnumerable<ValidationError> Visit(ReferencesFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value >= properties.MaxItems.Value)
            {
                yield return new ValidationError("Max items must be greater than min items.",
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(StringFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value.",
                    nameof(properties.Editor));
            }

            if ((properties.Editor == StringFieldEditor.Radio || properties.Editor == StringFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values.",
                    nameof(properties.AllowedValues));
            }

            if (properties.Pattern != null && !properties.Pattern.IsValidRegex())
            {
                yield return new ValidationError("Pattern is not a valid expression.",
                    nameof(properties.Pattern));
            }

            if (properties.MaxLength.HasValue && properties.MinLength.HasValue && properties.MinLength.Value >= properties.MaxLength.Value)
            {
                yield return new ValidationError("Max length must be greater than min length.",
                    nameof(properties.MinLength),
                    nameof(properties.MaxLength));
            }

            if (properties.AllowedValues != null && properties.AllowedValues.Count > 0 && (properties.MinLength.HasValue || properties.MaxLength.HasValue))
            {
                yield return new ValidationError("Either allowed values or min and max length can be defined.",
                    nameof(properties.AllowedValues),
                    nameof(properties.MinLength),
                    nameof(properties.MaxLength));
            }
        }

        public IEnumerable<ValidationError> Visit(TagsFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value >= properties.MaxItems.Value)
            {
                yield return new ValidationError("Max items must be greater than min items.",
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }
    }
}
