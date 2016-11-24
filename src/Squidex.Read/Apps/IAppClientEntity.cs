// ==========================================================================
//  IAppClientKeyEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Read.Apps
{
    public interface IAppClientEntity
    {
        string ClientName { get; }

        string ClientSecret { get; }

        DateTime ExpiresUtc { get; }
    }
}
