// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public interface IAssetFolderEntity : IEntity, IEntityWithVersion
{
    NamedId<DomainId> AppId { get; set; }

    string FolderName { get; set; }

    DomainId ParentId { get; set; }
}
