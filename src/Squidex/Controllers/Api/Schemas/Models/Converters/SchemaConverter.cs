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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

// ReSharper disable InvertIf

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public static class SchemaConverter
    {
        private static readonly Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>> Factories =
            new Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>>
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
                },
                {
                    typeof(ReferencesFieldProperties),
                    p => Convert((ReferencesFieldProperties)p)
                }
            };

        public static SchemaDto ToModel(this ISchemaEntity entity)
        {
            var dto = new SchemaDto { Properties = new SchemaPropertiesDto() };

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.Schema, dto);
            SimpleMapper.Map(entity.Schema.Properties, dto.Properties);

            return dto;
        }

        public static SchemaDetailsDto ToDetailsModel(this ISchemaEntity entity)
        {
            var dto = new SchemaDetailsDto { Properties = new SchemaPropertiesDto() };

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.Schema, dto);
            SimpleMapper.Map(entity.Schema.Properties, dto.Properties);

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

        public static CreateSchema ToCommand(this CreateSchemaDto dto)
        {
            var command = new CreateSchema();

            SimpleMapper.Map(dto, command);

            if (dto.Properties != null)
            {
                SimpleMapper.Map(dto.Properties, command.Properties);
            }

            if (dto.Fields != null)
            {
                foreach (var fieldDto in dto.Fields)
                {
                    var fieldProperties = fieldDto?.Properties.ToProperties();
                    var fieldInstance = SimpleMapper.Map(fieldDto, new CreateSchemaField { Properties = fieldProperties });

                    command.Fields.Add(fieldInstance);
                }
            }

            return command;
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

        private static FieldPropertiesDto Convert(ReferencesFieldProperties source)
        {
            var result = SimpleMapper.Map(source, new ReferencesFieldPropertiesDto());

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
