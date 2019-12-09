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
        public override IIndexStorage Storage => CreateStorage();

        private static IIndexStorage CreateStorage()
        {
            var storage = new FileIndexStorage();

            return storage;
        }
    }
}
