// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    internal sealed class DefaultFieldValueValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>>
    {
        private readonly ValidatorContext context;
        private readonly ValidatorFactory createFieldValidator;

        private DefaultFieldValueValidatorsFactory(ValidatorContext context, ValidatorFactory createFieldValidator)
        {
            this.context = context;
            this.createFieldValidator = createFieldValidator;
        }

        public static IEnumerable<IValidator> CreateValidators(ValidatorContext context, IField field, ValidatorFactory createFieldValidator)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(field, nameof(field));

            var visitor = new DefaultFieldValueValidatorsFactory(context, createFieldValidator);

            return field.Accept(visitor);
        }

        public IEnumerable<IValidator> Visit(IArrayField field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
            }

            var nestedValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(field.Fields.Count);

            foreach (var nestedField in field.Fields)
            {
                nestedValidators[nestedField.Name] = (false, createFieldValidator(nestedField));
            }

            yield return new CollectionItemValidator(new ObjectValidator<IJsonValue>(nestedValidators, false, "field"));
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }

            if (properties.MinValue.HasValue || properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<Instant>(properties.MinValue, properties.MaxValue);
            }
        }

        public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }

            if (properties.MinValue.HasValue || properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<double>(properties.MinValue, properties.MaxValue);
            }

            if (properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<double>(properties.AllowedValues);
            }
        }

        public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired)
            {
                yield return new RequiredStringValidator(true);
            }

            if (properties.MinLength.HasValue || properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(properties.MinLength, properties.MaxLength);
            }

            if (properties.MinCharacters.HasValue ||
                properties.MaxCharacters.HasValue ||
                properties.MinWords.HasValue ||
                properties.MaxWords.HasValue)
            {
                Func<string, string>? transform = null;

                switch (properties.ContentType)
                {
                    case StringContentType.Markdown:
                        transform = TextHelpers.Markdown2Text;
                        break;
                    case StringContentType.Html:
                        transform = TextHelpers.Html2Text;
                        break;
                }

                yield return new StringTextValidator(transform,
                   properties.MinCharacters,
                   properties.MaxCharacters,
                   properties.MinWords,
                   properties.MaxWords);
            }

            if (!string.IsNullOrWhiteSpace(properties.Pattern))
            {
                yield return new PatternValidator(properties.Pattern, properties.PatternMessage);
            }

            if (properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(properties.AllowedValues);
            }
        }

        public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties);

            if (isRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
            }

            if (properties.AllowedValues != null)
            {
                yield return new CollectionItemValidator(new AllowedValuesValidator<string>(properties.AllowedValues));
            }

            yield return new CollectionItemValidator(new RequiredStringValidator(true));
        }

        public IEnumerable<IValidator> Visit(IField<UIFieldProperties> field)
        {
            if (field is INestedField)
            {
                yield return NoValueValidator.Instance;
            }
        }

        private bool IsRequired(FieldProperties properties)
        {
            var isRequired = properties.IsRequired;

            if (context.Action == ValidationAction.Publish)
            {
                isRequired = isRequired || properties.IsRequiredOnPublish;
            }

            return isRequired;
        }
    }
}
