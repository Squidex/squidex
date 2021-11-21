// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Extensions.Text.ElasticSearch;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class ElasticTextIndexFixture
    {
        public ElasticSearchTextIndex Index { get; }

        public ElasticTextIndexFixture()
        {
            Index = new ElasticSearchTextIndex(
                TestConfig.Configuration["elastic:configuration"],
                TestConfig.Configuration["elastic:indexName"]);
            Index.InitializeAsync(default).Wait();
        }
    }
}
