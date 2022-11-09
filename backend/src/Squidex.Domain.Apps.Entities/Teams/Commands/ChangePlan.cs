// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Teams.Commands;

public sealed class ChangePlan : TeamCommand
{
    public bool FromCallback { get; set; }

    public string PlanId { get; set; }
}
