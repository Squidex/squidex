// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class AppImage
    {
        public string MimeType { get; }

        public string Etag { get; }

        public AppImage(string mimeType, string? etag = null)
        {
            Guard.NotNullOrEmpty(mimeType);

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
