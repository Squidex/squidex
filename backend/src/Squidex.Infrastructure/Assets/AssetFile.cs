﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Assets
{
    public sealed class AssetFile
    {
        private readonly Func<Stream> openAction;

        public string FileName { get; }

        public string MimeType { get; }

        public long FileSize { get; }

        public AssetFile(string fileName, string mimeType, long fileSize, Func<Stream> openAction)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNullOrEmpty(mimeType, nameof(mimeType));
            Guard.GreaterEquals(fileSize, 0, nameof(fileSize));

            FileName = fileName;
            FileSize = fileSize;

            MimeType = mimeType;

            this.openAction = openAction;
        }

        public Stream OpenRead()
        {
            return openAction();
        }
    }
}
