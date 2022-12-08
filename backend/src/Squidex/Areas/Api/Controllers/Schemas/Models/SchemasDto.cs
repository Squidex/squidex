// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class SchemasDto : Resource
{
    /// <summary>
    /// The schemas.
    /// </summary>
    public SchemaDto[] Items { get; set; }

    public static SchemasDto FromDomain(IList<ISchemaEntity> schemas, Resources resources)
    {
        var result = new SchemasDto
        {
            Items = schemas.Select(x => SchemaDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private SchemasDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<SchemasController>(x => nameof(x.GetSchemas), values));

        if (resources.CanCreateSchema)
        {
            AddPostLink("create",
                resources.Url<SchemasController>(x => nameof(x.PostSchema), values));
        }

        return this;
    }
}
