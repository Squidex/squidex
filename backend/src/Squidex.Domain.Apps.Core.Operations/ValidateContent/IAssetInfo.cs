// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public interface IAssetInfo
    {
        DomainId AssetId { get; }

        long FileSize { get; }

        string FileName { get; }

        string FileHash { get; }

        string Slug { get; }

        AssetMetadata Metadata { get; }

        AssetType Type { get; }
    }
}
