// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using Squidex.Assets;

namespace Squidex.Domain.Apps.Entities.TestHelpers
{
    public sealed class NoopAssetFile : AssetFile
    {
        public NoopAssetFile(string fileName = "image.png", string mimeType = "image/png", long fileSize = 1024)
            : base(fileName, mimeType, fileSize)
        {
        }

        public override Stream OpenRead()
        {
            return new MemoryStream();
        }
    }
}
