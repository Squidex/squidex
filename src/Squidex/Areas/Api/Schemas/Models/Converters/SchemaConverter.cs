// ==========================================================================
//  SchemaConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models.Converters
{
    public static class SchemaConverter
    {
        public static SchemaDto ToModel(this ISchemaEntity entity)
        {
            var dto = new SchemaDto { Properties = new SchemaPropertiesDto() };

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.SchemaDef, dto);
            SimpleMapper.Map(entity.SchemaDef.Properties, dto.Properties);

            return dto;
        }

        public static SchemaDetailsDto ToDetailsModel(this ISchemaEntity entity)
        {
            var dto = new SchemaDetailsDto { Properties = new SchemaPropertiesDto() };

            SimpleMapper.Map(entity, dto);
            SimpleMapper.Map(entity.SchemaDef, dto);
            SimpleMapper.Map(entity.SchemaDef.Properties, dto.Properties);

            dto.Fields = new List<FieldDto>();

            foreach (var field in entity.SchemaDef.Fields)
            {
                var fieldPropertiesDto = FieldPropertiesDtoFactory.Create(field.RawProperties);
                var fieldInstanceDto = SimpleMapper.Map(field,
                    new FieldDto
                    {
                        FieldId = field.Id,
                        Properties = fieldPropertiesDto,
                        Partitioning = field.Partitioning.Key
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
    }
}
