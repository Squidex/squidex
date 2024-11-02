// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class IndexFieldDto
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The sort order of the field.
    /// </summary>
    public SortOrder Order { get; set; }

    public static IndexFieldDto FromDomain(IndexField field)
    {
        return SimpleMapper.Map(field, new IndexFieldDto());
    }
}
