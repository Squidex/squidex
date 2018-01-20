// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models.Converters
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
                command.Properties = new SchemaProperties();

                SimpleMapper.Map(dto.Properties, command.Properties);
            }

            if (dto.Fields != null)
            {
                command.Fields = new List<CreateSchemaField>();

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
