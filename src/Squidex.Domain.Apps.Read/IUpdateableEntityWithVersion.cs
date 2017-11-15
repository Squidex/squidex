// ==========================================================================
//  IUpdateableEntityWithVersion.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Read
{
    public interface IUpdateableEntityWithVersion
    {
        long Version { get; set; }
    }
}
