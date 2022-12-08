// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Plans.Models;

public sealed class PlanChangedDto
{
    /// <summary>
    /// Optional redirect uri.
    /// </summary>
    public string? RedirectUri { get; set; }
}
