// ==========================================================================
//  IEntityWithAppRef.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Read
{
    public interface IEntityWithAppRef : IEntity
    {
        Guid AppId { get; }
    }
}