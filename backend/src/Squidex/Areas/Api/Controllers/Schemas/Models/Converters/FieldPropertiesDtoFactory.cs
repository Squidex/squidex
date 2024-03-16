// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Schemas.Models.Fields;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Converters
{
    internal sealed class FieldPropertiesDtoFactory : IFieldPropertiesVisitor<FieldPropertiesDto, None>
    {
        private static readonly FieldPropertiesDtoFactory Instance = new FieldPropertiesDtoFactory();

        private FieldPropertiesDtoFactory()
        {
        }

        public static FieldPropertiesDto Create(FieldProperties properties)
        {
            return properties.Accept(Instance, None.Value);
        }

        public FieldPropertiesDto Visit(ArrayFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new ArrayFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(AssetsFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new AssetsFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(BooleanFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new BooleanFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(ComponentFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new ComponentFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(ComponentsFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new ComponentsFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(DateTimeFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new DateTimeFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(GeolocationFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new GeolocationFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(JsonFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new JsonFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(NumberFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new NumberFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(ReferencesFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new ReferencesFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(StringFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new StringFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(TagsFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new TagsFieldPropertiesDto());
        }

        public FieldPropertiesDto Visit(UIFieldProperties properties, None args)
        {
            return SimpleMapper.Map(properties, new UIFieldPropertiesDto());
        }
    }
}
