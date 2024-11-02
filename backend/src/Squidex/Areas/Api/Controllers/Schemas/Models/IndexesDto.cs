// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class IndexesDto : Resource
{
    /// <summary>
    /// The indexes.
    /// </summary>
    public IndexDto[] Items { get; set; }

    public static IndexesDto FromDomain(List<IndexDefinition> indexes, Resources resources)
    {
        var result = new IndexesDto
        {
            Items = indexes.Select(x => IndexDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private IndexesDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, schema = resources.Schema };

        AddSelfLink(resources.Url<SchemaIndexesController>(x => nameof(x.GetIndexes), values));

        if (resources.CanManageIndexes(resources.Schema!))
        {
            AddPostLink("create",
                resources.Url<SchemaIndexesController>(x => nameof(x.PostIndex), values));
        }

        return this;
    }
}
