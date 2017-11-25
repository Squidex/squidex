// ==========================================================================
//  IUpdateableEntityWithAppRef.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Read
{
    public interface IUpdateableEntityWithAppRef
    {
        Guid AppId { get; set; }
    }
}
