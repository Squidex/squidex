// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerGrainTests_FS : TextIndexerGrainTestsBase
    {
        public override IDirectoryFactory DirectoryFactory => CreateFactory();

        private static IDirectoryFactory CreateFactory()
        {
            var directoryFactory = new FSDirectoryFactory();

            return directoryFactory;
        }
    }
}
