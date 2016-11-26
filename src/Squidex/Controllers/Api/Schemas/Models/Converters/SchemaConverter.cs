// ==========================================================================
//  SchemaConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Read.Schemas.Repositories;
using Dtos = Squidex.Controllers.Api.Schemas.Models.Fields;

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public static class SchemaConverter
    {
        private static readonly Dictionary<Type, Func<Field, FieldDto>> Factories = new Dictionary<Type, Func<Field, FieldDto>>
        {
            {
                typeof(NumberField),
                field =>
                {
                    var dto = new Dtos.NumberField();

                    SimpleMapper.Map(field, dto);
                    SimpleMapper.Map((NumberFieldProperties)field.RawProperties, dto);

                    return dto;
                }
            },
            {
                typeof(StringField),
                field =>
                {
                    var dto = new Dtos.StringField();

                    SimpleMapper.Map(field, dto);
                    SimpleMapper.Map((StringFieldProperties)field.RawProperties, dto);

                    return dto;
                }
            }
        };

        public static SchemaDetailsDto ToModel(this ISchemaEntityWithSchema entity)
        {
            var dto = new SchemaDetailsDto();

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.Schema, dto);
            SimpleMapper.Map(entity.Schema.Properties, dto);

            dto.Fields = new List<FieldDto>();

            foreach (var field in entity.Schema.Fields.Values)
            {
                var fieldDto = Factories[field.RawProperties.GetType()](field);

                dto.Fields.Add(fieldDto);
            }

            return dto;
        }
    }
}
