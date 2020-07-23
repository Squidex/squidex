// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Assets
{
    public abstract class AssetFile : IDisposable
    {
        public string FileName { get; }

        public string MimeType { get; }

        public long FileSize { get; }

        protected AssetFile(string fileName, string mimeType, long fileSize)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNullOrEmpty(mimeType, nameof(mimeType));
            Guard.GreaterEquals(fileSize, 0, nameof(fileSize));

            FileName = fileName;
            FileSize = fileSize;

            MimeType = mimeType;
        }

        public virtual void Dispose()
        {
        }

        public abstract Stream OpenRead();
    }
}
