// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetItemEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion,
        IEntityWithTags,
        IAssetInfo
    {
        NamedId<Guid> AppId { get; }

        Guid ParentId { get; }

        string FolderName { get; }

        string MimeType { get; }

        bool IsFolder { get; }

        long FileVersion { get; }
    }
}
