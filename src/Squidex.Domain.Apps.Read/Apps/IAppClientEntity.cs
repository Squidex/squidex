// ==========================================================================
//  IAppClientEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;

namespace Squidex.Domain.Apps.Read.Apps
{
    public interface IAppClientEntity
    {
        string Name { get; }

        string Secret { get; }

        AppClientPermission Permission { get; }
    }
}
