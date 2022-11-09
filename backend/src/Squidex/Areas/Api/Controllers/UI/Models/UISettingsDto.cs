// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.UI.Models;

public sealed class UISettingsDto
{
    /// <summary>
    /// True when the user can create apps.
    /// </summary>
    public bool CanCreateApps { get; set; }

    /// <summary>
    /// True when the user can create teams.
    /// </summary>
    public bool CanCreateTeams { get; set; }
}
