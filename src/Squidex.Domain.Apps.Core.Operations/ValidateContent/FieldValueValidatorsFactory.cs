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
    public sealed class FieldValueValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>>
    {
        private static readonly FieldValueValidatorsFactory Instance = new FieldValueValidatorsFactory();

        private FieldValueValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(IField field)
        {
            Guard.NotNull(field);

            return field.Accept(Instance);
        }

        public IEnumerable<IValidator> Visit(IArrayField field)
        {
            if (field.Properties.IsRequired || field.Properties.MinItems.HasValue || field.Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(field.Properties.IsRequired, field.Properties.MinItems, field.Properties.MaxItems);
            }

            var nestedSchema = new Dictionary<string, (bool IsOptional, IValidator Validator)>(field.Fields.Count);

            foreach (var nestedField in field.Fields)
            {
                nestedSchema[nestedField.Name] = (false, nestedField.CreateValidator());
            }

            yield return new CollectionItemValidator(new ObjectValidator<IJsonValue>(nestedSchema, false, "field"));
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field)
        {
            if (field.Properties.IsRequired || field.Properties.MinItems.HasValue || field.Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(field.Properties.IsRequired, field.Properties.MinItems, field.Properties.MaxItems);
            }

            if (!field.Properties.AllowDuplicates)
            {
                yield return new UniqueValuesValidator<Guid>();
            }

            yield return new AssetsValidator(field.Properties);
        }

        public IEnumerable<IValidator> Visit(IField<BooleanFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<DateTimeFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (field.Properties.MinValue.HasValue || field.Properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<Instant>(field.Properties.MinValue, field.Properties.MaxValue);
            }
        }

        public IEnumerable<IValidator> Visit(IField<GeolocationFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<JsonFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<NumberFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (field.Properties.MinValue.HasValue || field.Properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<double>(field.Properties.MinValue, field.Properties.MaxValue);
            }

            if (field.Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<double>(field.Properties.AllowedValues);
            }

            if (field.Properties.IsUnique)
            {
                yield return new UniqueValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<ReferencesFieldProperties> field)
        {
            if (field.Properties.IsRequired || field.Properties.MinItems.HasValue || field.Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(field.Properties.IsRequired, field.Properties.MinItems, field.Properties.MaxItems);
            }

            if (!field.Properties.AllowDuplicates)
            {
                yield return new UniqueValuesValidator<Guid>();
            }

            yield return new ReferencesValidator(field.Properties.SchemaIds);
        }

        public IEnumerable<IValidator> Visit(IField<StringFieldProperties> field)
        {
            if (field.Properties.IsRequired)
            {
                yield return new RequiredStringValidator(true);
            }

            if (field.Properties.MinLength.HasValue || field.Properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(field.Properties.MinLength, field.Properties.MaxLength);
            }

            if (!string.IsNullOrWhiteSpace(field.Properties.Pattern))
            {
                yield return new PatternValidator(field.Properties.Pattern, field.Properties.PatternMessage);
            }

            if (field.Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(field.Properties.AllowedValues);
            }

            if (field.Properties.IsUnique)
            {
                yield return new UniqueValidator();
            }
        }

        public IEnumerable<IValidator> Visit(IField<TagsFieldProperties> field)
        {
            if (field.Properties.IsRequired || field.Properties.MinItems.HasValue || field.Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(field.Properties.IsRequired, field.Properties.MinItems, field.Properties.MaxItems);
            }

            if (field.Properties.AllowedValues != null)
            {
                yield return new CollectionItemValidator(new AllowedValuesValidator<string>(field.Properties.AllowedValues));
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
    }
}
