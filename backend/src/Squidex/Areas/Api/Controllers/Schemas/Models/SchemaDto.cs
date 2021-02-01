// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
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
        /// The name of the schema. Unique within the app.
        /// </summary>
        [LocalizedRequired]
        [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Name { get; set; }

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
        public bool IsSingleton { get; set; }

        /// <summary>
        /// Indicates if the schema is published.
        /// </summary>
        public bool IsPublished { get; set; }

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

        public static SchemaDto FromSchema(ISchemaEntity schema, Resources controller)
        {
            var result = new SchemaDto();

            SimpleMapper.Map(schema, result);
            SimpleMapper.Map(schema.SchemaDef, result);
            SimpleMapper.Map(schema.SchemaDef.Properties, result.Properties);

            result.CreateLinks(controller);

            return result;
        }

        protected virtual void CreateLinks(Resources resources)
        {
            var values = new { app = resources.App, name = Name };

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
                AddPutLink("update/sync", resources.Url<SchemasController>(x => nameof(x.PutSchemaSync), values));
                AddPutLink("update/urls", resources.Url<SchemasController>(x => nameof(x.PutPreviewUrls), values));
                AddPutLink("update/rules", resources.Url<SchemasController>(x => nameof(x.PutRules), values));
                AddPutLink("update/category", resources.Url<SchemasController>(x => nameof(x.PutCategory), values));
            }

            if (resources.CanUpdateSchemaScripts(Name))
            {
                AddPutLink("update/scripts", resources.Url<SchemasController>(x => nameof(x.PutScripts), values));
            }

            if (resources.CanDeleteSchema(Name))
            {
                AddDeleteLink("delete", resources.Url<SchemasController>(x => nameof(x.DeleteSchema), values));
            }
        }
    }
}
