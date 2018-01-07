// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public interface IAppEntity : IEntity, IEntityWithVersion
    {
        string Name { get; }

        AppPlan Plan { get; }

        AppClients Clients { get; }

        AppPatterns Patterns { get; }

        AppContributors Contributors { get; }

        LanguagesConfig LanguagesConfig { get; }
    }
}
