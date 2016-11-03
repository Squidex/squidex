// ==========================================================================
//  IAppContributorEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Apps;

namespace Squidex.Read.Apps
{
    public interface IAppContributorEntity
    {
        string SubjectId { get; }
        
        PermissionLevel Permission { get; }
    }
}
