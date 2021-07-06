// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public class SchemaDto : Resource
    {
        /// <summary>
        /// The id of the schema.
        /// </summary>
        public DomainId Id { get; set; }

        /// <summary>
        /// The user that has created the schema.
        /// </summary>
        [LocalizedRequired]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the schema.
        /// </summary>
        [LocalizedRequired]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The name of the schema. Unique within the app.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the schema.
        /// </summary>
        public SchemaType Type { get; set; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The schema properties.
        /// </summary>
        [LocalizedRequired]
        public SchemaPropertiesDto Properties { get; set; } = new SchemaPropertiesDto();

        /// <summary>
        /// Indicates if the schema is a singleton.
        /// </summary>
        [Obsolete("Use 'type' field now.")]
        public bool IsSingleton
        {
            get => Type == SchemaType.Singleton;
        }

        /// <summary>
        /// Indicates if the schema is published.
        /// </summary>
        public bool IsPublished { get; set; }

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

        /// <summary>
        /// The scripts.
        /// </summary>
        [LocalizedRequired]
        public SchemaScriptsDto Scripts { get; set; } = new SchemaScriptsDto();

        /// <summary>
        /// The preview Urls.
        /// </summary>
        [LocalizedRequired]
        public ImmutableDictionary<string, string> PreviewUrls { get; set; }

        /// <summary>
        /// The name of fields that are used in content lists.
        /// </summary>
        [LocalizedRequired]
        public FieldNames FieldsInLists { get; set; }

        /// <summary>
        /// The name of fields that are used in content references.
        /// </summary>
        [LocalizedRequired]
        public FieldNames FieldsInReferences { get; set; }

        /// <summary>
        /// The field rules.
        /// </summary>
        public List<FieldRuleDto> FieldRules { get; set; }

        /// <summary>
        /// The list of fields.
        /// </summary>
        [LocalizedRequired]
        public List<FieldDto> Fields { get; set; }

        public static SchemaDto FromSchema(ISchemaEntity schema, Resources resources)
        {
            var result = new SchemaDto();

            SimpleMapper.Map(schema, result);
            SimpleMapper.Map(schema.SchemaDef, result);
            SimpleMapper.Map(schema.SchemaDef.Scripts, result.Scripts);
            SimpleMapper.Map(schema.SchemaDef.Properties, result.Properties);

            result.FieldRules = schema.SchemaDef.FieldRules.Select(FieldRuleDto.FromFieldRule).ToList();

            result.Fields = new List<FieldDto>();

            foreach (var field in schema.SchemaDef.Fields)
            {
                result.Fields.Add(FieldDto.FromField(field));
            }

            result.CreateLinks(resources);

            return result;
        }

        protected virtual void CreateLinks(Resources resources)
        {
            var values = new { app = resources.App, schema = Name };

            var allowUpdate = resources.CanUpdateSchema(Name);

            AddSelfLink(resources.Url<SchemasController>(x => nameof(x.GetSchema), values));

            if (resources.CanReadContent(Name))
            {
                AddGetLink("contents", resources.Url<ContentsController>(x => nameof(x.GetContents), values));
            }

            if (resources.CanCreateContent(Name))
            {
                AddPostLink("contents/create", resources.Url<ContentsController>(x => nameof(x.PostContent), values));
                AddPostLink("contents/create/publish", resources.Url<ContentsController>(x => nameof(x.PostContent), values) + "?publish=true");
            }

            if (resources.CanPublishSchema(Name))
            {
                if (IsPublished)
                {
                    AddPutLink("unpublish", resources.Url<SchemasController>(x => nameof(x.UnpublishSchema), values));
                }
                else
                {
                    AddPutLink("publish", resources.Url<SchemasController>(x => nameof(x.PublishSchema), values));
                }
            }

            if (allowUpdate)
            {
                AddPostLink("fields/add", resources.Url<SchemaFieldsController>(x => nameof(x.PostField), values));

                AddPutLink("fields/ui", resources.Url<SchemaFieldsController>(x => nameof(x.PutSchemaUIFields), values));
                AddPutLink("fields/order", resources.Url<SchemaFieldsController>(x => nameof(x.PutSchemaFieldOrdering), values));

                AddPutLink("update", resources.Url<SchemasController>(x => nameof(x.PutSchema), values));
                AddPutLink("update/category", resources.Url<SchemasController>(x => nameof(x.PutCategory), values));
                AddPutLink("update/rules", resources.Url<SchemasController>(x => nameof(x.PutRules), values));
                AddPutLink("update/sync", resources.Url<SchemasController>(x => nameof(x.PutSchemaSync), values));
                AddPutLink("update/urls", resources.Url<SchemasController>(x => nameof(x.PutPreviewUrls), values));
            }

            if (resources.CanUpdateSchemaScripts(Name))
            {
                AddPutLink("update/scripts", resources.Url<SchemasController>(x => nameof(x.PutScripts), values));
            }

            if (resources.CanDeleteSchema(Name))
            {
                AddDeleteLink("delete", resources.Url<SchemasController>(x => nameof(x.DeleteSchema), values));
            }

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
