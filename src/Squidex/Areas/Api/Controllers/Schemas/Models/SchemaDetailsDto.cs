// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Areas.Api.Controllers.Schemas.Models.Converters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemaDetailsDto
    {
        private static readonly Dictionary<string, string> EmptyPreviewUrls = new Dictionary<string, string>();

        /// <summary>
        /// The id of the schema.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the schema. Unique within the app.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Indicates if the schema is a singleton.
        /// </summary>
        public bool IsSingleton { get; set; }

        /// <summary>
        /// Indicates if the schema is published.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The scripts.
        /// </summary>
        public SchemaScriptsDto Scripts { get; set; } = new SchemaScriptsDto();

        /// <summary>
        /// The preview Urls.
        /// </summary>
        public Dictionary<string, string> PreviewUrls { get; set; } = EmptyPreviewUrls;

        /// <summary>
        /// The list of fields.
        /// </summary>
        [Required]
        public List<FieldDto> Fields { get; set; }

        /// <summary>
        /// The schema properties.
        /// </summary>
        [Required]
        public SchemaPropertiesDto Properties { get; set; } = new SchemaPropertiesDto();

        /// <summary>
        /// The user that has created the schema.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the schema.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the schema has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the schema has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The version of the schema.
        /// </summary>
        public long Version { get; set; }

        public static SchemaDetailsDto FromSchema(ISchemaEntity schema)
        {
            var response = new SchemaDetailsDto();

            SimpleMapper.Map(schema, response);
            SimpleMapper.Map(schema.SchemaDef, response);
            SimpleMapper.Map(schema.SchemaDef.Scripts, response.Scripts);
            SimpleMapper.Map(schema.SchemaDef.Properties, response.Properties);

            if (schema.SchemaDef.PreviewUrls.Count > 0)
            {
                response.PreviewUrls = new Dictionary<string, string>(schema.SchemaDef.PreviewUrls);
            }

            response.Fields = new List<FieldDto>();

            foreach (var field in schema.SchemaDef.Fields)
            {
                var fieldPropertiesDto = FieldPropertiesDtoFactory.Create(field.RawProperties);
                var fieldDto =
                    SimpleMapper.Map(field,
                        new FieldDto
                        {
                            FieldId = field.Id,
                            Properties = fieldPropertiesDto,
                            Partitioning = field.Partitioning.Key
                        });

                if (field is IArrayField arrayField)
                {
                    fieldDto.Nested = new List<NestedFieldDto>();

                    foreach (var nestedField in arrayField.Fields)
                    {
                        var nestedFieldPropertiesDto = FieldPropertiesDtoFactory.Create(nestedField.RawProperties);
                        var nestedFieldDto =
                            SimpleMapper.Map(nestedField,
                                new NestedFieldDto
                                {
                                    FieldId = nestedField.Id,
                                    Properties = nestedFieldPropertiesDto
                                });

                        fieldDto.Nested.Add(nestedFieldDto);
                    }
                }

                response.Fields.Add(fieldDto);
            }

            return response;
        }
    }
}
