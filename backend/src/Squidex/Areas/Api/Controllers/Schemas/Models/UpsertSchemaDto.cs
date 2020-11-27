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
    public abstract class UpsertSchemaDto
    {
        /// <summary>
        /// The optional properties.
        /// </summary>
        public SchemaPropertiesDto? Properties { get; set; }

        /// <summary>
        /// The optional scripts.
        /// </summary>
        public SchemaScriptsDto? Scripts { get; set; }

        /// <summary>
        /// The names of the fields that should be used in references.
        /// </summary>
        public string[]? FieldsInReferences { get; set; }

        /// <summary>
        /// The names of the fields that should be shown in lists, including meta fields.
        /// </summary>
        public string[]? FieldsInLists { get; set; }

        /// <summary>
        /// Optional fields.
        /// </summary>
        public UpsertSchemaFieldDto[]? Fields { get; set; }

        /// <summary>
        /// The optional preview urls.
        /// </summary>
        public Dictionary<string, string>? PreviewUrls { get; set; }

        /// <summary>
        /// The category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Set it to true to autopublish the schema.
        /// </summary>
        public bool IsPublished { get; set; }

        public static T ToCommand<T, TSoure>(TSoure dto, T command) where T : SchemaCommand, IUpsertCommand where TSoure : UpsertSchemaDto
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

            if (dto.FieldsInLists != null)
            {
                command.FieldsInLists = new FieldNames(dto.FieldsInLists);
            }

            if (dto.FieldsInReferences != null)
            {
                command.FieldsInReferences = new FieldNames(dto.FieldsInReferences);
            }

            if (dto.Fields?.Length > 0)
            {
                var fields = new List<UpsertSchemaField>();

                foreach (var rootFieldDto in dto.Fields)
                {
                    var rootProperties = rootFieldDto?.Properties?.ToProperties();
                    var rootField = new UpsertSchemaField { Properties = rootProperties! };

                    if (rootFieldDto != null)
                    {
                        SimpleMapper.Map(rootFieldDto, rootField);

                        if (rootFieldDto?.Nested?.Length > 0)
                        {
                            var nestedFields = new List<UpsertSchemaNestedField>();

                            foreach (var nestedFieldDto in rootFieldDto.Nested)
                            {
                                var nestedProperties = nestedFieldDto?.Properties?.ToProperties();
                                var nestedField = new UpsertSchemaNestedField { Properties = nestedProperties! };

                                if (nestedFieldDto != null)
                                {
                                    SimpleMapper.Map(nestedFieldDto, nestedField);
                                }

                                nestedFields.Add(nestedField);
                            }

                            rootField.Nested = nestedFields.ToArray();
                        }
                    }

                    fields.Add(rootField);
                }

                command.Fields = fields.ToArray();
            }

            return command;
        }
    }
}
