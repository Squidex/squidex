// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerTests_Assets : TextIndexerTestsBase
    {
        public override IIndexerFactory Factory { get; } = new LuceneIndexFactory(TestStorages.Assets());
    }
}
