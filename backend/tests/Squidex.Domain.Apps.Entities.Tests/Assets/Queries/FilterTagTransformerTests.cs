// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.Queries
{
    public class FilterTagTransformerTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly DomainId appId = DomainId.NewGuid();

        [Fact]
        public async Task Should_normalize_tags()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId, TagGroups.Assets, A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string> { ["name1"] = "id1" });

            var source = ClrFilter.Eq("tags", "name1");

            var result = await FilterTagTransformer.TransformAsync(source, appId, tagService);

            Assert.Equal("tags == 'id1'", result!.ToString());
        }

        [Fact]
        public async Task Should_not_fail_when_tags_not_found()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId, TagGroups.Assets, A<HashSet<string>>.That.Contains("name1")))
                .Returns(new Dictionary<string, string>());

            var source = ClrFilter.Eq("tags", "name1");

            var result = await FilterTagTransformer.TransformAsync(source, appId, tagService);

            Assert.Equal("tags == 'name1'", result!.ToString());
        }

        [Fact]
        public async Task Should_not_normalize_other_field()
        {
            var source = ClrFilter.Eq("other", "value");

            var result = await FilterTagTransformer.TransformAsync(source, appId, tagService);

            Assert.Equal("other == 'value'", result!.ToString());

            A.CallTo(() => tagService.GetTagIdsAsync(appId, A<string>._, A<HashSet<string>>._))
                .MustNotHaveHappened();
        }
    }
}
