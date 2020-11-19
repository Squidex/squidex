// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record AppImage
    {
        public string MimeType { get; }

        public string Etag { get; }

        public AppImage(string mimeType, string? etag = null)
        {
            Guard.NotNullOrEmpty(mimeType, nameof(mimeType));

            MimeType = mimeType;

            if (string.IsNullOrWhiteSpace(etag))
            {
                Etag = RandomHash.Simple();
            }
            else
            {
                Etag = etag;
            }
        }
    }
}
