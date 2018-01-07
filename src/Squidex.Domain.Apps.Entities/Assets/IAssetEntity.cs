// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ValidateContent;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public interface IAssetEntity :
        IEntity,
        IEntityWithAppRef,
        IEntityWithCreatedBy,
        IEntityWithLastModifiedBy,
        IEntityWithVersion,
        IAssetInfo
    {
        string MimeType { get; }

        long FileVersion { get; }
    }
}
