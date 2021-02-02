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
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    internal sealed class DefaultFieldValueValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>, DefaultFieldValueValidatorsFactory.Args>
    {
        private static readonly DefaultFieldValueValidatorsFactory Instance = new DefaultFieldValueValidatorsFactory();

        public readonly struct Args
        {
            public readonly ValidatorContext Context;

            public readonly ValidatorFactory Factory;

            public Args(ValidatorContext context, ValidatorFactory factory)
            {
                Context = context;

                Factory = factory;
            }
        }

        private DefaultFieldValueValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(ValidatorContext context, IField field, ValidatorFactory createFieldValidator)
        {
            var args = new Args(context, createFieldValidator);

            return field.Accept(Instance, args);
        }

        public IEnumerable<IValidator> Visit(IArrayField field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

            if (isRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(isRequired, properties.MinItems, properties.MaxItems);
            }

            var nestedValidators = new Dictionary<string, (bool IsOptional, IValidator Validator)>(field.Fields.Count);

            foreach (var nestedField in field.Fields)
            {
                nestedValidators[nestedField.Name] = (false, args.Factory(nestedField));
            }

            yield return new CollectionItemValidator(new ObjectValidator<IJsonValue>(nestedValidators, false, "field"));
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field, Args args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }

            if (properties.MinValue.HasValue || properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<Instant>(properties.MinValue, properties.MaxValue);
            }
        }

        public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

            if (isRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

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

        public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field, Args args)
        {
            yield break;
        }

        public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

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

        public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field, Args args)
        {
            var properties = field.Properties;

            var isRequired = IsRequired(properties, args.Context);

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

        public IEnumerable<IValidator> Visit(IField<UIFieldProperties> field, Args args)
        {
            if (field is INestedField)
            {
                yield return NoValueValidator.Instance;
            }
        }

        private static bool IsRequired(FieldProperties properties, ValidatorContext context)
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
