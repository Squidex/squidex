// ==========================================================================
//  IContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Read.Contents
{
    public interface IContentEntity : IEntityWithAppRef, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        Status Status { get; }

        NamedContentData Data { get; }
    }
}
