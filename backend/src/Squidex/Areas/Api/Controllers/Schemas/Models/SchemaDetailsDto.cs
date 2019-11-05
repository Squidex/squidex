// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public SchemaScriptsDto Scripts { get; set; } = new SchemaScriptsDto();

        /// <summary>
        /// The preview Urls.
        /// </summary>
        [Required]
        public Dictionary<string, string> PreviewUrls { get; set; } = EmptyPreviewUrls;

        /// <summary>
        /// The name of fields that are used in content lists.
        /// </summary>
        [Required]
        public FieldNames FieldsInLists { get; set; }

        /// <summary>
        /// The name of fields that are used in content references.
        /// </summary>
        [Required]
        public FieldNames FieldsInReferences { get; set; }

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
                result.Fields.Add(FieldDto.FromField(field));
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
