// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public interface IAssetInfo
    {
        Guid AssetId { get; }

        long FileSize { get; }

        string FileName { get; }

        string FileHash { get; }

        string Slug { get; }

        AssetMetadata Metadata { get; }

        AssetType Type { get; }
    }
}
