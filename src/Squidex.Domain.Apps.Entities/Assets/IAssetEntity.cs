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
    public interface IAssetEntity :
        IEntity,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion,
        IAssetInfo
    {
        NamedId<Guid> AppId { get; }

        string MimeType { get; }

        long FileVersion { get; }
    }
}
