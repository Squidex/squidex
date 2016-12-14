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

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public static class SchemaConverter
    {
        private static readonly Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>> Factories = new Dictionary<Type, Func<FieldProperties, FieldPropertiesDto>>
        {
            {
                typeof(NumberFieldProperties),
                p => SimpleMapper.Map((NumberFieldProperties)p, new NumberFieldPropertiesDto())
            },
            {
                typeof(StringFieldProperties),
                p => SimpleMapper.Map((StringFieldProperties)p, new StringFieldPropertiesDto())
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
                var fieldPropertiesDto = Factories[field.RawProperties.GetType()](field.RawProperties);

                var fieldDto = new FieldDto
                {
                    Name = field.Name,
                    IsHidden = field.IsHidden,
                    IsDisabled = field.IsDisabled,
                    Properties = fieldPropertiesDto
                };

                dto.Fields.Add(fieldDto);
            }

            return dto;
        }
    }
}
