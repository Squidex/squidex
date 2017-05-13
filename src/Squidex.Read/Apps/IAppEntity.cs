// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Core;

namespace Squidex.Read.Apps
{
    public interface IAppEntity : IEntity, IEntityWithVersion
    {
        string Name { get; }

        LanguagesConfig LanguagesConfig { get; }

        IReadOnlyCollection<IAppClientEntity> Clients { get; }

        IReadOnlyCollection<IAppContributorEntity> Contributors { get; }

        PartitionResolver PartitionResolver { get; }
    }
}
