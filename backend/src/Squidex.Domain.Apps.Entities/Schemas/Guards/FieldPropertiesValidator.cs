// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

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
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxItems), nameof(properties.MinItems)),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(AssetsFieldProperties properties)
        {
            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxItems), nameof(properties.MinItems)),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }

            if (properties.MaxHeight.HasValue && properties.MinHeight.HasValue && properties.MinHeight.Value > properties.MaxHeight.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxHeight), nameof(properties.MinHeight)),
                    nameof(properties.MaxHeight),
                    nameof(properties.MinHeight));
            }

            if (properties.MaxWidth.HasValue && properties.MinWidth.HasValue && properties.MinWidth.Value > properties.MaxWidth.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxWidth), nameof(properties.MinWidth)),
                    nameof(properties.MaxWidth),
                    nameof(properties.MinWidth));
            }

            if (properties.MaxSize.HasValue && properties.MinSize.HasValue && properties.MinSize.Value >= properties.MaxSize.Value)
            {
                yield return new ValidationError(Not.GreaterThan(nameof(properties.MaxSize), nameof(properties.MinSize)),
                    nameof(properties.MaxSize),
                    nameof(properties.MinSize));
            }

            if (properties.AspectWidth.HasValue != properties.AspectHeight.HasValue)
            {
                yield return new ValidationError(Not.BothDefined(nameof(properties.AspectWidth), nameof(properties.AspectHeight)),
                    nameof(properties.AspectWidth),
                    nameof(properties.AspectHeight));
            }
        }

        public IEnumerable<ValidationError> Visit(BooleanFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(DateTimeFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.DefaultValue), nameof(properties.MinValue)),
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.LessEqualsThan(nameof(properties.DefaultValue), nameof(properties.MaxValue)),
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.GreaterThan(nameof(properties.MaxValue), nameof(properties.MinValue)),
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.CalculatedDefaultValue.HasValue)
            {
                if (!properties.CalculatedDefaultValue.Value.IsEnumValue())
                {
                    yield return new ValidationError(Not.Valid(nameof(properties.CalculatedDefaultValue)),
                        nameof(properties.CalculatedDefaultValue));
                }

                if (properties.DefaultValue.HasValue)
                {
                    yield return new ValidationError(T.Get("schemas.dateTimeCalculatedDefaultAndDefaultError"),
                        nameof(properties.CalculatedDefaultValue),
                        nameof(properties.DefaultValue));
                }
            }
        }

        public IEnumerable<ValidationError> Visit(GeolocationFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
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
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == NumberFieldEditor.Radio || properties.Editor == NumberFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError(T.Get("schemas.stringEditorsNeedAllowedValuesError"),
                    nameof(properties.AllowedValues));
            }

            if (properties.DefaultValue.HasValue && properties.MinValue.HasValue && properties.DefaultValue.Value < properties.MinValue.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.DefaultValue), nameof(properties.MinValue)),
                    nameof(properties.DefaultValue));
            }

            if (properties.DefaultValue.HasValue && properties.MaxValue.HasValue && properties.DefaultValue.Value > properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.LessEqualsThan(nameof(properties.DefaultValue), nameof(properties.MaxValue)),
                    nameof(properties.DefaultValue));
            }

            if (properties.MaxValue.HasValue && properties.MinValue.HasValue && properties.MinValue.Value >= properties.MaxValue.Value)
            {
                yield return new ValidationError(Not.GreaterThan(nameof(properties.MaxValue), nameof(properties.MinValue)),
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.AllowedValues != null && properties.AllowedValues.Count > 0 && (properties.MinValue.HasValue || properties.MaxValue.HasValue))
            {
                yield return new ValidationError(T.Get("schemas.string.eitherMinMaxOrAllowedValuesError"),
                    nameof(properties.AllowedValues),
                    nameof(properties.MinValue),
                    nameof(properties.MaxValue));
            }

            if (properties.InlineEditable && properties.Editor == NumberFieldEditor.Radio)
            {
                yield return new ValidationError(T.Get("schemas.number.inlineEditorError"),
                    nameof(properties.InlineEditable),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(ReferencesFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }

            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxItems), nameof(properties.MinItems)),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }

            if (properties.ResolveReference && properties.MaxItems != 1)
            {
                yield return new ValidationError(T.Get("schemas.references.resolveError"),
                    nameof(properties.ResolveReference),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(StringFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == StringFieldEditor.Radio || properties.Editor == StringFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError(T.Get("schemas.stringEditorsNeedAllowedValuesError"),
                    nameof(properties.AllowedValues));
            }

            if (properties.Pattern != null && !properties.Pattern.IsValidRegex())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Pattern)),
                    nameof(properties.Pattern));
            }

            if (properties.MaxLength.HasValue && properties.MinLength.HasValue && properties.MinLength.Value > properties.MaxLength.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxLength), nameof(properties.MinLength)),
                    nameof(properties.MinLength),
                    nameof(properties.MaxLength));
            }

            if (properties.AllowedValues != null && properties.AllowedValues.Count > 0 && (properties.MinLength.HasValue || properties.MaxLength.HasValue))
            {
                yield return new ValidationError(T.Get("schemas.number.eitherMinMaxOrAllowedValuesError"),
                    nameof(properties.AllowedValues),
                    nameof(properties.MinLength),
                    nameof(properties.MaxLength));
            }

            if (properties.InlineEditable && properties.Editor != StringFieldEditor.Dropdown && properties.Editor != StringFieldEditor.Input && properties.Editor != StringFieldEditor.Slug)
            {
                yield return new ValidationError(T.Get("schemas.string.inlineEditorError"),
                    nameof(properties.InlineEditable),
                    nameof(properties.Editor));
            }
        }

        public IEnumerable<ValidationError> Visit(TagsFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }

            if ((properties.Editor == TagsFieldEditor.Checkboxes || properties.Editor == TagsFieldEditor.Dropdown) && (properties.AllowedValues == null || properties.AllowedValues.Count == 0))
            {
                yield return new ValidationError(T.Get("schemas.tags.editorNeedsAllowedValues"),
                    nameof(properties.AllowedValues));
            }

            if (properties.MaxItems.HasValue && properties.MinItems.HasValue && properties.MinItems.Value > properties.MaxItems.Value)
            {
                yield return new ValidationError(Not.GreaterEqualsThan(nameof(properties.MaxItems), nameof(properties.MinItems)),
                    nameof(properties.MinItems),
                    nameof(properties.MaxItems));
            }
        }

        public IEnumerable<ValidationError> Visit(UIFieldProperties properties)
        {
            if (!properties.Editor.IsEnumValue())
            {
                yield return new ValidationError(Not.Valid(nameof(properties.Editor)),
                    nameof(properties.Editor));
            }
        }
    }
}
