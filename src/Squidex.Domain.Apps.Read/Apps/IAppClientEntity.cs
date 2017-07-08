// ==========================================================================
//  IAppClientKeyEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Apps
{
    public interface IAppClientEntity
    {
        string Id { get; }

        string Name { get; }

        string Secret { get; }
    }
}
