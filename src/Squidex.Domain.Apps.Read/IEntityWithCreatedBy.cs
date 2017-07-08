// ==========================================================================
//  IEntityWithCreatedBy.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read
{
    public interface IEntityWithCreatedBy
    {
        RefToken CreatedBy { get; set; }
    }
}
