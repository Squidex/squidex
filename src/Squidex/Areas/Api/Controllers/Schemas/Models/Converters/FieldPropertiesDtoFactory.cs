﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Areas.Api.Controllers.Schemas.Models.Fields;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Converters
{
    public class FieldPropertiesDtoFactory : IFieldPropertiesVisitor<FieldPropertiesDto>
    {
        private static readonly FieldPropertiesDtoFactory Instance = new FieldPropertiesDtoFactory();

        private FieldPropertiesDtoFactory()
        {
        }

        public static FieldPropertiesDto Create(FieldProperties properties)
        {
            return properties.Accept(Instance);
        }

        public FieldPropertiesDto Visit(ArrayFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new ArrayFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(AssetsFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new AssetsFieldPropertiesDto());

            result.AllowedExtensions = properties.AllowedExtensions?.ToArray();

            return result;
        }

        public FieldPropertiesDto Visit(BooleanFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new BooleanFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(DateTimeFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new DateTimeFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(GeolocationFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new GeolocationFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(JsonFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new JsonFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(NumberFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new NumberFieldPropertiesDto());

            result.AllowedValues = properties.AllowedValues?.ToArray();

            return result;
        }

        public FieldPropertiesDto Visit(ReferencesFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new ReferencesFieldPropertiesDto());

            result.SchemaIds = properties.SchemaIds?.ToArray();

            return result;
        }

        public FieldPropertiesDto Visit(StringFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new StringFieldPropertiesDto());

            result.AllowedValues = properties.AllowedValues?.ToArray();

            return result;
        }

        public FieldPropertiesDto Visit(TagsFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new TagsFieldPropertiesDto());

            result.AllowedValues = properties.AllowedValues?.ToArray();

            return result;
        }

        public FieldPropertiesDto Visit(UIFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new UIFieldPropertiesDto());
        }
    }
}
