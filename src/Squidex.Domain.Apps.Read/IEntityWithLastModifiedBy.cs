// ==========================================================================
//  ITrackLastModifiedByEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read
{
    public interface IEntityWithLastModifiedBy
    {
        RefToken LastModifiedBy { get; set; }
    }
}
