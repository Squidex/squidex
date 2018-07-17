// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class TagGrainTests
    {
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IPersistence<TagGrain.State> persistence = A.Fake<IPersistence<TagGrain.State>>();
        private readonly TagGrain sut;

        public TagGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<string>.Ignored, A<Func<TagGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new TagGrain(store);
            sut.OnActivateAsync(string.Empty).Wait();
        }

        [Fact]
        public async Task Should_add_tags_to_grain()
        {
            var result1 = await sut.NormalizeTagsAsync(HashSet.Of("tag1", "tag2"), null);
            var result2 = await sut.NormalizeTagsAsync(HashSet.Of("tag2", "tag3"), null);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1"] = 1,
                ["tag2"] = 2,
                ["tag3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_not_add_tags_if_already_added()
        {
            var result1 = await sut.NormalizeTagsAsync(HashSet.Of("tag1", "tag2"), null);
            var result2 = await sut.NormalizeTagsAsync(HashSet.Of("tag1", "tag2", "tag3"), result1);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1"] = 1,
                ["tag2"] = 1,
                ["tag3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_remove_tags_from_grain()
        {
            var result1 = await sut.NormalizeTagsAsync(HashSet.Of("tag1", "tag2"), null);
            var result2 = await sut.NormalizeTagsAsync(HashSet.Of("tag2", "tag3"), null);

            await sut.NormalizeTagsAsync(null, result1);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag2"] = 1,
                ["tag3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_resolve_tag_names()
        {
            var tagIds = await sut.NormalizeTagsAsync(HashSet.Of("tag1", "tag2"), null);
            var tagNames = await sut.GetTagIdsAsync(HashSet.Of("tag1", "tag2", "invalid1"));

            Assert.Equal(tagIds, tagNames);
        }
    }
}
