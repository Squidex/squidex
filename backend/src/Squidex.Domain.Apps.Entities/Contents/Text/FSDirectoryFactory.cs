// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Lucene.Net.Store;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class FSDirectoryFactory : IDirectoryFactory
    {
        public LuceneDirectory Create(Guid schemaId)
        {
            var folderName = $"Indexes/{schemaId}";

            var tempFolder = Path.Combine(Path.GetTempPath(), folderName);

            return FSDirectory.Open(tempFolder);
        }
    }
}
