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
using Squidex.Infrastructure.Orleans;
using Xunit;

#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class TagGrainTests
    {
        private readonly IGrainState<TagGrain.State> grainState = A.Fake<IGrainState<TagGrain.State>>();
        private readonly string id = DomainId.NewGuid().ToString();
        private readonly TagGrain sut;

        public TagGrainTests()
        {
            A.CallTo(() => grainState.ClearAsync())
                .Invokes(() => grainState.Value = new TagGrain.State());

            sut = new TagGrain(grainState);
            sut.ActivateAsync(id).Wait();
        }

        [Fact]
        public async Task Should_delete_and_reset_state_if_cleaning()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("name2", "name3"), null);
            await sut.ClearAsync();

            var allTags = await sut.GetTagsAsync();

            Assert.Empty(allTags);

            A.CallTo(() => grainState.ClearAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_rebuild_tags()
        {
            var tags = new TagsExport
            {
                ["id1"] = new Tag { Name = "name1", Count = 1 },
                ["id2"] = new Tag { Name = "name2", Count = 2 },
                ["id3"] = new Tag { Name = "name3", Count = 6 }
            };

            await sut.RebuildAsync(tags);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 2,
                ["name3"] = 6
            }, allTags);

            Assert.Same(tags, await sut.GetExportableTagsAsync());
        }

        [Fact]
        public async Task Should_add_tags_to_grain()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("name2", "name3"), null);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 2,
                ["name3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_not_add_tags_if_already_added()
        {
            var result1 = await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            var result2 = await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2", "name3"), new HashSet<string>(result1.Values));

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 1,
                ["name3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_remove_tags_from_grain()
        {
            var result1 = await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            var result2 = await sut.NormalizeTagsAsync(HashSet.Of("name2", "name3"), null);

            await sut.NormalizeTagsAsync(null, new HashSet<string>(result1.Values));

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["name2"] = 1,
                ["name3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_resolve_tag_names()
        {
            var tagIds = await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            var tagNames = await sut.GetTagIdsAsync(HashSet.Of("name1", "name2", "invalid1"));

            Assert.Equal(tagIds, tagNames);
        }
    }
}
