// ==========================================================================
//  ITrackCreatedByEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Read
{
    public interface ITrackCreatedByEntity
    {
        RefToken CreatedBy { get; set; }
    }
}
