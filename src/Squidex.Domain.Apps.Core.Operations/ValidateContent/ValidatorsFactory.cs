// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidatorsFactory : IFieldVisitor<IEnumerable<IValidator>>
    {
        private static readonly ValidatorsFactory Instance = new ValidatorsFactory();

        private ValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(IField field)
        {
            Guard.NotNull(field, nameof(field));

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
                nestedSchema[nestedField.Name] = (false, new FieldValidator(nestedField.Accept(this).ToArray(), nestedField));
            }

            yield return new CollectionItemValidator(new ObjectValidator<IJsonValue>(nestedSchema, false, "field", JsonValue.Null));
        }

        public IEnumerable<IValidator> Visit(IField<AssetsFieldProperties> field)
        {
            if (field.Properties.IsRequired || field.Properties.MinItems.HasValue || field.Properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator(field.Properties.IsRequired, field.Properties.MinItems, field.Properties.MaxItems);
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

            if (field.Properties.SchemaId != Guid.Empty)
            {
                yield return new ReferencesValidator(field.Properties.SchemaId);
            }
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
    }
}
