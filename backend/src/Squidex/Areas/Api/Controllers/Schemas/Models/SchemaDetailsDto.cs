// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemaDetailsDto : SchemaDto
    {
        private static readonly Dictionary<string, string> EmptyPreviewUrls = new Dictionary<string, string>();

        /// <summary>
        /// The scripts.
        /// </summary>
        [LocalizedRequired]
        public SchemaScriptsDto Scripts { get; set; } = new SchemaScriptsDto();

        /// <summary>
        /// The preview Urls.
        /// </summary>
        [LocalizedRequired]
        public Dictionary<string, string> PreviewUrls { get; set; } = EmptyPreviewUrls;

        /// <summary>
        /// The name of fields that are used in content lists.
        /// </summary>
        [LocalizedRequired]
        public List<string> FieldsInLists { get; set; }

        /// <summary>
        /// The name of fields that are used in content references.
        /// </summary>
        [LocalizedRequired]
        public List<string> FieldsInReferences { get; set; }

        /// <summary>
        /// The field rules.
        /// </summary>
        public List<FieldRuleDto> FieldRules { get; set; }

        /// <summary>
        /// The list of fields.
        /// </summary>
        [LocalizedRequired]
        public List<FieldDto> Fields { get; set; }

        public static SchemaDetailsDto FromSchemaWithDetails(ISchemaEntity schema, Resources resources)
        {
            var result = new SchemaDetailsDto();

            SimpleMapper.Map(schema, result);
            SimpleMapper.Map(schema.SchemaDef, result);
            SimpleMapper.Map(schema.SchemaDef.Scripts, result.Scripts);
            SimpleMapper.Map(schema.SchemaDef.Properties, result.Properties);

            result.FieldsInLists = schema.SchemaDef.FieldsInLists.ToList();
            result.FieldsInReferences = schema.SchemaDef.FieldsInReferences.ToList();

            result.FieldRules = schema.SchemaDef.FieldRules.Select(FieldRuleDto.FromFieldRule).ToList();

            if (schema.SchemaDef.PreviewUrls.Count > 0)
            {
                result.PreviewUrls = new Dictionary<string, string>(schema.SchemaDef.PreviewUrls);
            }

            result.Fields = new List<FieldDto>();

            foreach (var field in schema.SchemaDef.Fields)
            {
                result.Fields.Add(FieldDto.FromField(field));
            }

            result.CreateLinks(resources);

            return result;
        }

        protected override void CreateLinks(Resources resources)
        {
            base.CreateLinks(resources);

            var allowUpdate = resources.CanUpdateSchema(Name);

            if (Fields != null)
            {
                foreach (var nested in Fields)
                {
                    nested.CreateLinks(resources, Name, allowUpdate);
                }
            }
        }
    }
}
