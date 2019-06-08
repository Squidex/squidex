// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Areas.Api.Controllers.Schemas.Models.Converters;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemaDetailsDto : SchemaDto
    {
        private static readonly Dictionary<string, string> EmptyPreviewUrls = new Dictionary<string, string>();

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

        public static SchemaDetailsDto FromSchemaWithDetails(ISchemaEntity schema, ApiController controller, string app)
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

            return CreateLinks(response, controller, app);
        }
    }
}
