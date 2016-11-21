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
    public interface IAppClientKeyEntity
    {
        string ClientKey { get; }

        DateTime ExpiresUtc { get; }
    }
}
