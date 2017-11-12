// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Read.Apps
{
    public interface IAppEntity : IEntity, IEntityWithVersion
    {
        string Name { get; }

        string PlanId { get; }

        string PlanOwner { get; }

        AppClients Clients { get; }

        AppContributors Contributors { get; }

        LanguagesConfig LanguagesConfig { get; }
    }
}
