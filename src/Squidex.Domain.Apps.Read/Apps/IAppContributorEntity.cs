// ==========================================================================
//  IAppContributorEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Read.Apps
{
    public interface IAppContributorEntity
    {
        string ContributorId { get; }

        PermissionLevel Permission { get; }
    }
}
