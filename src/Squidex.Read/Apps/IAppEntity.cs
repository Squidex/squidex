// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Read.Apps
{
    public interface IAppEntity : IEntity
    {
        string Name { get; }

        IEnumerable<IAppContributorEntity> Contributors { get; }
    }
}
