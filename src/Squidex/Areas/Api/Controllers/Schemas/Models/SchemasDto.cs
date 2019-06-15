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
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SchemasDto : Resource
    {
        /// <summary>
        /// The schemas.
        /// </summary>
        public SchemaDto[] Items { get; set; }

        public string ToEtag()
        {
            return Items.ToManyEtag();
        }

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
            return this;
        }
    }
}
