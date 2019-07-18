// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Guards
{
    public sealed class FieldPropertiesValidator : IFieldPropertiesVisitor<IEnumerable<ValidationError>>
    {
        private static readonly FieldPropertiesValidator Instance = new FieldPropertiesValidator();

        private FieldPropertiesValidator()
        {
        }

        public static IEnumerable<ValidationError> Validate(FieldProperties properties)
        {
            if (properties != null)
            {
                if (!properties.IsForApi() && properties.IsListField)
                {
                    yield return new ValidationError("UI field cannot be a list field.", nameof(properties.IsListField));
                }

                if (!properties.IsForApi() && properties.IsReferenceField)
                {
                    yield return new ValidationError("UI field cannot be a reference field.", nameof(properties.IsReferenceField));
                }

                foreach (var error in properties.Accept(Instance))
                {
                    yield return error;
                }
            }
        }

        public IEnumerable<ValidationError> Visit(ArrayFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max items", "min items"),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(AssetsFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max items", "min items"),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }

            if (properties.MaxHeight.HasValue && properties.MinHeight.HasValue && properties.MinHeight.Value > properties.MaxHeight.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max height", "min height"),
                    nameof(properties.MaxHeight),
                    nameof(properties.MinHeight));
            }

            if (properties.MaxWidth.HasValue && properties.MinWidth.HasValue && properties.MinWidth.Value > properties.MaxWidth.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max width", "min width"),
                    nameof(properties.MaxWidth),
                    nameof(properties.MinWidth));
            }

            if (properties.MaxSize.HasValue && properties.MinSize.HasValue && properties.MinSize.Value >= properties.MaxSize.Value)
            {
                yield return new ValidationError(Not.GreaterThan("Max size", "min size"),
                    nameof(properties.MaxSize),
                    nameof(properties.MinSize));
            }

            if (properties.AspectWidth.HasValue != properties.AspectHeight.HasValue)
            {
                yield return new ValidationError(Not.Defined2("Aspect width", "aspect height"),
                    nameof(properties.AspectWidth),
                    nameof(properties.AspectHeight));
            }
        }

        public IEnumerable<ValidationError> Visit(BooleanFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(DateTimeFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Default value", "min value"),
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.LessEquals("Default value", "max value"),
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.GreaterThan("Max value", "min value"),
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.CalculatedDefaultValue.HasValue)
            {
                if (!properties.CalculatedDefaultValue.Value.IsEnumValue())
                {
                    yield return new ValidationError(Not.Valid("Calculated default value"),
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
                yield return new ValidationError(Not.Valid("Editor"),
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
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == NumberFieldEditor.Radio || properties.Editor == NumberFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values.",
                    nameof(properties.AllowedValues));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Default value", "min value"),
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.LessEquals("Default value", "max value"),
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.GreaterThan("Max value", "min value"),
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

            if (properties.InlineEditable && properties.Editor != NumberFieldEditor.Input && properties.Editor != NumberFieldEditor.Dropdown)
            {
                yield return new ValidationError("Inline editing is only allowed for dropdowns and input fields.",
                    nameof(properties.InlineEditable),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(ReferencesFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }

            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max items", "min items"),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(StringFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == StringFieldEditor.Radio || properties.Editor == StringFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values.",
                    nameof(properties.AllowedValues));
            }

            if (properties.Pattern != null && !properties.Pattern.IsValidRegex())
            {
                yield return new ValidationError(Not.Valid("Pattern"),
                    nameof(properties.Pattern));
            }

            if (properties.MaxLength.HasValue && properties.MinLength.HasValue && properties.MinLength.Value > properties.MaxLength.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max length", "min length"),
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

            if (properties.InlineEditable && properties.Editor != StringFieldEditor.Dropdown && properties.Editor != StringFieldEditor.Input && properties.Editor != StringFieldEditor.Slug)
            {
                yield return new ValidationError("Inline editing is only allowed for dropdowns, slugs and input fields.",
                    nameof(properties.InlineEditable),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(TagsFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == TagsFieldEditor.Checkboxes || properties.Editor == TagsFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError("Checkboxes or dropdown list need allowed values.",
                    nameof(properties.AllowedValues));
            }

            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEquals("Max items", "min items"),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(UIFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid("Editor"),
                    nameof(properties.Editor));
            }
        }
    }
}
