// ==========================================================================
//  ITrackLastModifiedByEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Read
{
    public interface ITrackLastModifiedByEntity
    {
        RefToken LastModifiedBy { get; set; }
    }
}
