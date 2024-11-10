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

[OpenApiRequest]
public sealed class CreateIndexDto
{
    /// <summary>
    /// The index fields.
    /// </summary>
    [LocalizedRequired]
    public List<IndexFieldDto> Fields { get; set; }

    public IndexDefinition ToIndex()
    {
        var result = new IndexDefinition();

        foreach (var field in Fields)
        {
            result.Add(new IndexField(field.Name, field.Order));
        }

        return result;
    }
}
