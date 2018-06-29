// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class CreateSchemaDto
    {
        /// <summary>
        /// The name of the schema.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The optional properties.
        /// </summary>
        public SchemaPropertiesDto Properties { get; set; }

        /// <summary>
        /// Optional fields.
        /// </summary>
        public List<CreateSchemaFieldDto> Fields { get; set; }

        /// <summary>
        /// Set to true to allow a single content item only.
        /// </summary>
        public bool Singleton { get; set; }

        /// <summary>
        /// Set it to true to autopublish the schema.
        /// </summary>
        public bool Publish { get; set; }

        public CreateSchema ToCommand()
        {
            var command = new CreateSchema();

            SimpleMapper.Map(this, command);

            if (Properties != null)
            {
                command.Properties = new SchemaProperties();

                SimpleMapper.Map(Properties, command.Properties);
            }

            if (Fields != null)
            {
                command.Fields = new List<CreateSchemaField>();

                foreach (var fieldDto in Fields)
                {
                    var rootProperties = fieldDto?.Properties.ToProperties();
                    var rootField = SimpleMapper.Map(fieldDto, new CreateSchemaField { Properties = rootProperties });

                    if (fieldDto.Nested != null)
                    {
                        rootField.Nested = new List<CreateSchemaNestedField>();

                        foreach (var nestedFieldDto in fieldDto.Nested)
                        {
                            var nestedProperties = nestedFieldDto?.Properties.ToProperties();
                            var nestedField = SimpleMapper.Map(nestedFieldDto, new CreateSchemaNestedField { Properties = nestedProperties });

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
