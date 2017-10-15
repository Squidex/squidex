// ==========================================================================
//  FieldPropertiesDtoFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models.Converters
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

        public FieldPropertiesDto Visit(AssetsFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new AssetsFieldPropertiesDto());
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

        public FieldPropertiesDto Visit(ReferencesFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new ReferencesFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(TagsFieldProperties properties)
        {
            return SimpleMapper.Map(properties, new TagsFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(NumberFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new NumberFieldPropertiesDto());

            if (properties.AllowedValues != null)
            {
                result.AllowedValues = properties.AllowedValues.ToArray();
            }

            return result;
        }

        public FieldPropertiesDto Visit(StringFieldProperties properties)
        {
            var result = SimpleMapper.Map(properties, new StringFieldPropertiesDto());

            if (properties.AllowedValues != null)
            {
                result.AllowedValues = properties.AllowedValues.ToArray();
            }

            return result;
        }
    }
}
