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

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class ValidatorsFactory : IFieldPropertiesVisitor<IEnumerable<IValidator>>
    {
        private static readonly ValidatorsFactory Instance = new ValidatorsFactory();

        private ValidatorsFactory()
        {
        }

        public static IEnumerable<IValidator> CreateValidators(Field field)
        {
            Guard.NotNull(field, nameof(field));

            return field.RawProperties.Accept(Instance);
        }

        public IEnumerable<IValidator> Visit(AssetsFieldProperties properties)
        {
            if (properties.IsRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<Guid>(properties.IsRequired, properties.MinItems, properties.MaxItems);
            }

            yield return new AssetsValidator(properties);
        }

        public IEnumerable<IValidator> Visit(BooleanFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(DateTimeFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (properties.MinValue.HasValue || properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<Instant>(properties.MinValue, properties.MaxValue);
            }
        }

        public IEnumerable<IValidator> Visit(GeolocationFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(JsonFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        public IEnumerable<IValidator> Visit(NumberFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (properties.MinValue.HasValue || properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<double>(properties.MinValue, properties.MaxValue);
            }

            if (properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<double>(properties.AllowedValues.ToArray());
            }
        }

        public IEnumerable<IValidator> Visit(ReferencesFieldProperties properties)
        {
            if (properties.IsRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<Guid>(properties.IsRequired, properties.MinItems, properties.MaxItems);
            }

            if (properties.SchemaId != Guid.Empty)
            {
                yield return new ReferencesValidator(properties.SchemaId);
            }
        }

        public IEnumerable<IValidator> Visit(StringFieldProperties properties)
        {
            if (properties.IsRequired)
            {
                yield return new RequiredStringValidator();
            }

            if (properties.MinLength.HasValue || properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(properties.MinLength, properties.MaxLength);
            }

            if (!string.IsNullOrWhiteSpace(properties.Pattern))
            {
                yield return new PatternValidator(properties.Pattern, properties.PatternMessage);
            }

            if (properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(properties.AllowedValues.ToArray());
            }
        }

        public IEnumerable<IValidator> Visit(TagsFieldProperties properties)
        {
            if (properties.IsRequired || properties.MinItems.HasValue || properties.MaxItems.HasValue)
            {
                yield return new CollectionValidator<string>(properties.IsRequired, properties.MinItems, properties.MaxItems);
            }

            yield return new CollectionItemValidator<string>(new RequiredStringValidator());
        }
    }
}
