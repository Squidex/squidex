// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class IndexDto : Resource
{
    /// <summary>
    /// The name of the index.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The index fields.
    /// </summary>
    [LocalizedRequired]
    public List<IndexFieldDto> Fields { get; set; }

    public static IndexDto FromDomain(IndexDefinition index, Resources resources)
    {
        var result = new IndexDto
        {
            Name = index.ToName(),
            Fields = index.Select(IndexFieldDto.FromDomain).ToList(),
        };

        return result.CreateLinks(resources);
    }

    private IndexDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, schema = resources.Schema, name = Name };

        if (resources.CanManageIndexes(resources.Schema!))
        {
            AddDeleteLink("delete",
                resources.Url<SchemaIndexesController>(x => nameof(x.DeleteIndex), values));
        }

        return this;
    }
}
