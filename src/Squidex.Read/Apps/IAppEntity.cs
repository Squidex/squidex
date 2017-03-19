// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Read.Apps
{
    public interface IAppEntity : IEntity, IEntityWithVersion
    {
        string Name { get; }

        Language MasterLanguage { get; }

        IReadOnlyCollection<IAppClientEntity> Clients { get; }

        IReadOnlyCollection<IAppContributorEntity> Contributors { get; }

        IReadOnlyCollection<Language> Languages { get; }
    }
}
