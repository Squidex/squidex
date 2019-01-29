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
        /// The optional scripts.
        /// </summary>
        public SchemaScriptsDto Scripts { get; set; }

        /// <summary>
        /// Optional fields.
        /// </summary>
        public List<UpsertSchemaFieldDto> Fields { get; set; }

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
        public bool IsPublished { get; set; }

        public static TCommand ToCommand<TCommand, TDto>(TDto dto, TCommand command) where TCommand : UpsertCommand where TDto : UpsertDto
        {
            SimpleMapper.Map(dto, command);

            if (dto.Properties != null)
            {
                command.Properties = new SchemaProperties();

                SimpleMapper.Map(dto.Properties, command.Properties);
            }

            if (dto.Scripts != null)
            {
                command.Scripts = new SchemaScripts();

                SimpleMapper.Map(dto.Scripts, command.Scripts);
            }

            if (dto.Fields != null)
            {
                command.Fields = new List<UpsertSchemaField>();

                foreach (var rootFieldDto in dto.Fields)
                {
                    var rootProps = rootFieldDto?.Properties.ToProperties();
                    var rootField = new UpsertSchemaField { Properties = rootProps };

                    SimpleMapper.Map(rootFieldDto, rootField);

                    if (rootFieldDto.Nested?.Count > 0)
                    {
                        rootField.Nested = new List<UpsertSchemaNestedField>();

                        foreach (var nestedFieldDto in rootFieldDto.Nested)
                        {
                            var nestedProps = nestedFieldDto?.Properties.ToProperties();
                            var nestedField = new UpsertSchemaNestedField { Properties = nestedProps };

                            SimpleMapper.Map(nestedFieldDto, nestedField);

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
