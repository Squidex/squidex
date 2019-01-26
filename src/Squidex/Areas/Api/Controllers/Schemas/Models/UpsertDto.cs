// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public abstract class UpsertDto
    {
        /// <summary>
        /// The optional properties.
        /// </summary>
        public SchemaPropertiesDto Properties { get; set; }

        /// <summary>
        /// Optional fields.
        /// </summary>
        public List<UpsertSchemaFieldDto> Fields { get; set; }

        /// <summary>
        /// The optional scripts.
        /// </summary>
        public Dictionary<string, string> Scripts { get; set; }

        /// <summary>
        /// The optional preview urls.
        /// </summary>
        public Dictionary<string, string> PreviewUrls { get; set; }

        /// <summary>
        /// The category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Set it to true to autopublish the schema.
        /// </summary>
        public bool Publish { get; set; }

        public static TCommand ToCommand<TCommand, TDto>(TDto dto, TCommand command) where TCommand : UpsertCommand where TDto : UpsertDto
        {
            SimpleMapper.Map(dto, command);

            if (dto.Properties != null)
            {
                command.Properties = new SchemaProperties();

                SimpleMapper.Map(dto.Properties, command.Properties);
            }

            if (dto.Fields != null)
            {
                command.Fields = new List<UpsertSchemaField>();

                foreach (var fieldDto in dto.Fields)
                {
                    var rootProperties = fieldDto?.Properties.ToProperties();
                    var rootField = SimpleMapper.Map(fieldDto, new UpsertSchemaField { Properties = rootProperties });

                    if (fieldDto.Nested != null)
                    {
                        rootField.Nested = new List<UpsertSchemaNestedField>();

                        foreach (var nestedFieldDto in fieldDto.Nested)
                        {
                            var nestedProperties = nestedFieldDto?.Properties.ToProperties();
                            var nestedField = SimpleMapper.Map(nestedFieldDto, new UpsertSchemaNestedField { Properties = nestedProperties });

                            rootField.Nested.Add(nestedField);
                        }
                    }

                    command.Fields.Add(rootField);
                }
            }

            return command;
        }
    }
}
