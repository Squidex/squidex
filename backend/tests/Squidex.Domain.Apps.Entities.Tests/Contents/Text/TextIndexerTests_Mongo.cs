// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Mongo : TextIndexerTestsBase
    {
        public override IIndexerFactory Factory { get; } = new LuceneIndexFactory(TestStorages.MongoDB());
    }
}
