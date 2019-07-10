// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemasDto : Resource
    {
        /// <summary>
        /// The schemas.
        /// </summary>
        public SchemaDto[] Items { get; set; }

        public static SchemasDto FromSchemas(IList<ISchemaEntity> schemas, ApiController controller, string app)
        {
            var result = new SchemasDto
            {
                Items = schemas.Select(x => SchemaDto.FromSchema(x, controller, app)).ToArray()
            };

            return result.CreateLinks(controller, app);
        }

        private SchemasDto CreateLinks(ApiController controller, string app)
        {
            var values = new { app };

            AddSelfLink(controller.Url<SchemasController>(x => nameof(x.GetSchemas), values));

            if (controller.HasPermission(Permissions.AppSchemasCreate, app))
            {
                AddPostLink("create", controller.Url<SchemasController>(x => nameof(x.PostSchema), values));
            }

            return this;
        }
    }
}
