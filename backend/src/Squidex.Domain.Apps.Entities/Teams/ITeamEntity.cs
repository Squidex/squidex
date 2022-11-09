// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;

namespace Squidex.Domain.Apps.Entities.Teams;

public interface ITeamEntity :
    IEntity,
    IEntityWithCreatedBy,
    IEntityWithLastModifiedBy,
    IEntityWithVersion
{
    string Name { get; }

    Contributors Contributors { get; }

    AssignedPlan? Plan { get; }
}
