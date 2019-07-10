// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        string Name { get; }

        Roles Roles { get; }

        AppPlan Plan { get; }

        AppClients Clients { get; }

        AppPatterns Patterns { get; }

        AppContributors Contributors { get; }

        LanguagesConfig LanguagesConfig { get; }

        Workflows Workflows { get; }

        bool IsArchived { get; }
    }
}
