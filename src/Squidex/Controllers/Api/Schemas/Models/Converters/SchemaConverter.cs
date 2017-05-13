// ==========================================================================
//  SchemaConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Schemas;

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public static class SchemaConverter
    {
        private static readonly Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>> Factories = new Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>>
        {
            {
                typeof(NumberFieldProperties),
                p => Convert((NumberFieldProperties)p)
            },
            {
                typeof(DateTimeFieldProperties),
                p => Convert((DateTimeFieldProperties)p)
            },
            {
                typeof(JsonFieldProperties),
                p => Convert((JsonFieldProperties)p)
            },
            {
                typeof(StringFieldProperties),
                p => Convert((StringFieldProperties)p)
            },
            {
                typeof(BooleanFieldProperties),
                p => Convert((BooleanFieldProperties)p)
            },
            {
                typeof(GeolocationFieldProperties),
                p => Convert((GeolocationFieldProperties)p)
            },
            {
                typeof(AssetsFieldProperties),
                p => Convert((AssetsFieldProperties)p)
            }
        };

        public static SchemaDetailsDto ToModel(this ISchemaEntityWithSchema entity)
        {
            var dto = new SchemaDetailsDto();

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.Schema, dto);
            SimpleMapper.Map(entity.Schema.Properties, dto);

            dto.Fields = new List<FieldDto>();

            foreach (var field in entity.Schema.Fields)
            {
                var fieldPropertiesDto = Factories[field.RawProperties.GetType()](field.RawProperties);
                var fieldInstanceDto = SimpleMapper.Map(field,
                    new FieldDto
                    {
                        FieldId = field.Id,
                        Properties = fieldPropertiesDto,
                        Partitioning = field.Paritioning.Key
                    });

                dto.Fields.Add(fieldInstanceDto);
            }

            return dto;
        }

        private static FieldPropertiesDto Convert(BooleanFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new BooleanFieldPropertiesDto());

            return result;
        }

        private static FieldPropertiesDto Convert(DateTimeFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new DateTimeFieldPropertiesDto());

            return result;
        }

        private static FieldPropertiesDto Convert(JsonFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new JsonFieldPropertiesDto());

            return result;
        }

        private static FieldPropertiesDto Convert(GeolocationFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new GeolocationFieldPropertiesDto());

            return result;
        }

        private static FieldPropertiesDto Convert(AssetsFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new AssetsFieldPropertiesDto());

            return result;
        }

        private static FieldPropertiesDto Convert(StringFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new StringFieldPropertiesDto());

            if (source.AllowedValues != null)
            {
                result.AllowedValues = source.AllowedValues.ToArray();
            }

            return result;
        }

        private static FieldPropertiesDto Convert(NumberFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new NumberFieldPropertiesDto());

            if (source.AllowedValues != null)
            {
                result.AllowedValues = source.AllowedValues.ToArray();
            }

            return result;
        }
    }
}
