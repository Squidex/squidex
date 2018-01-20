// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public interface IAssetInfo
    {
        Guid AssetId { get; }

        long FileSize { get; }

        bool IsImage { get; }

        int? PixelWidth { get; }

        int? PixelHeight { get; }

        string FileName { get; }
    }
}
