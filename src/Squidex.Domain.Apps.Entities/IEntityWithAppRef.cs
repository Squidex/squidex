// ==========================================================================
//  IEntityWithAppRef.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities
{
    public interface IEntityWithAppRef
    {
        Guid AppId { get; }
    }
}