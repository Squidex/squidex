// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public interface IAssetInfo : IWithId
    {
        long FileSize { get; }

        bool IsImage { get; }

        int? PixelWidth { get; }

        int? PixelHeight { get; }

        string FileName { get; }

        string FileHash { get; }

        string Slug { get; }
    }
}
