// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentEntity :
        IEntity,
        IEntityWithAppRef,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion
    {
        Status Status { get; }

        NamedContentData Data { get; }
    }
}
