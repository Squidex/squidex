// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetEntity :
    IEntity,
    IEntityWithCreatedBy,
    IEntityWithLastModifiedBy,
    IEntityWithVersion,
    IEntityWithTags,
    IAssetInfo
{
    NamedId<DomainId> AppId { get; }

    DomainId ParentId { get; }

    bool IsProtected { get; }

    long FileVersion { get; }
}
