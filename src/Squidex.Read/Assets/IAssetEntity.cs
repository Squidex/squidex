// ==========================================================================
//  IAssetEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read.Assets
{
    public interface IAssetEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        string MimeType { get; }

        string Name { get; }

        long FileSize { get; }
    }
}
