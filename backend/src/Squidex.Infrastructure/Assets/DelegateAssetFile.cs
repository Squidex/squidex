// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Assets
{
    public sealed class DelegateAssetFile : AssetFile
    {
        private readonly Func<Stream> openStream;

        public DelegateAssetFile(string fileName, string mimeType, long fileSize, Func<Stream> openStream)
            : base(fileName, mimeType, fileSize)
        {
            this.openStream = openStream;
        }

        public override Stream OpenRead()
        {
            return openStream();
        }
    }
}
