// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Plans.Models;

public enum PlansLockedReason
{
    /// <summary>
    /// The user can change the plan
    /// </summary>
    None,

    /// <summary>
    /// The user is not the owner.
    /// </summary>
    NotOwner,

    /// <summary>
    /// The user does not have permission to change the plan
    /// </summary>
    NoPermission,

    /// <summary>
    /// The plan is managed by the team.
    /// </summary>
    ManagedByTeam
}
