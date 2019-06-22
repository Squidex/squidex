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
using Squidex.Shared;
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
            var result = new SchemaDetailsDto();

            SimpleMapper.Map(schema, result);
            SimpleMapper.Map(schema.SchemaDef, result);
            SimpleMapper.Map(schema.SchemaDef.Scripts, result.Scripts);
            SimpleMapper.Map(schema.SchemaDef.Properties, result.Properties);

            if (schema.SchemaDef.PreviewUrls.Count > 0)
            {
                result.PreviewUrls = new Dictionary<string, string>(schema.SchemaDef.PreviewUrls);
            }

            result.Fields = new List<FieldDto>();

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

                result.Fields.Add(fieldDto);
            }

            result.CreateLinks(controller, app);

            return result;
        }

        protected override void CreateLinks(ApiController controller, string app)
        {
            base.CreateLinks(controller, app);

            var allowUpdate = controller.HasPermission(Permissions.AppSchemasUpdate, app, Name);

            if (Fields != null)
            {
                foreach (var nested in Fields)
                {
                    nested.CreateLinks(controller, app, Name, allowUpdate);
                }
            }
        }
    }
}
