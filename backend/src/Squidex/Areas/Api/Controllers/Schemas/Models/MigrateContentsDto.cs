// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

[OpenApiRequest]
public sealed class MigrateContentsDto
{
    /// <summary>
    /// True, to migrate the draft versions. Default: true.
    /// </summary>
    public bool? MigrateDraft { get; set; }

    /// <summary>
    /// True, to migrate the published versions. Default: true.
    /// </summary>
    public bool? MigratePublished { get; set; }
}
