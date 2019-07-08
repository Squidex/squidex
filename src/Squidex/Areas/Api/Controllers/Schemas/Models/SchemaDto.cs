// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public class SchemaDto : Resource
    {
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
        /// The schema properties.
        /// </summary>
        [Required]
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

        public static SchemaDto FromSchema(ISchemaEntity schema, ApiController controller, string app)
        {
            var result = new SchemaDto();

            SimpleMapper.Map(schema, result);
            SimpleMapper.Map(schema.SchemaDef, result);
            SimpleMapper.Map(schema.SchemaDef.Properties, result.Properties);

            result.CreateLinks(controller, app);

            return result;
        }

        protected virtual void CreateLinks(ApiController controller, string app)
        {
            var values = new { app, name = Name };

            var allowUpdate = controller.HasPermission(Permissions.AppSchemasUpdate, app, Name);

            AddSelfLink(controller.Url<SchemasController>(x => nameof(x.GetSchema), values));

            if (controller.HasPermission(Permissions.AppContentsRead, app, Name))
            {
                AddGetLink("contents", controller.Url<ContentsController>(x => nameof(x.GetContents), values));
            }

            if (controller.HasPermission(Permissions.AppSchemasPublish, app, Name))
            {
                if (IsPublished)
                {
                    AddPutLink("unpublish", controller.Url<SchemasController>(x => nameof(x.UnpublishSchema), values));
                }
                else
                {
                    AddPutLink("publish", controller.Url<SchemasController>(x => nameof(x.PublishSchema), values));
                }
            }

            if (allowUpdate)
            {
                AddPutLink("fields/order", controller.Url<SchemaFieldsController>(x => nameof(x.PutSchemaFieldOrdering), values));

                AddPutLink("update", controller.Url<SchemasController>(x => nameof(x.PutSchema), values));
                AddPutLink("update/category", controller.Url<SchemasController>(x => nameof(x.PutCategory), values));
                AddPutLink("update/sync", controller.Url<SchemasController>(x => nameof(x.PutSchemaSync), values));
                AddPutLink("update/urls", controller.Url<SchemasController>(x => nameof(x.PutPreviewUrls), values));

                AddPostLink("fields/add", controller.Url<SchemaFieldsController>(x => nameof(x.PostField), values));
            }

            if (controller.HasPermission(Permissions.AppSchemasScripts, app, Name))
            {
                AddPutLink("update/scripts", controller.Url<SchemasController>(x => nameof(x.PutScripts), values));
            }

            if (controller.HasPermission(Permissions.AppSchemasDelete, app, Name))
            {
                AddDeleteLink("delete", controller.Url<SchemasController>(x => nameof(x.DeleteSchema), values));
            }
        }
    }
}
