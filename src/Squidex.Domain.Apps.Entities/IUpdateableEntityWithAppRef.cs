// ==========================================================================
//  IUpdateableEntityWithAppRef.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities
{
    public interface IUpdateableEntityWithAppRef
    {
        Guid AppId { get; set; }
    }
}
