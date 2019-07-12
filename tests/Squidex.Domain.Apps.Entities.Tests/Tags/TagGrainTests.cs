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
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class TagGrainTests
    {
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IPersistence<TagGrain.GrainState> persistence = A.Fake<IPersistence<TagGrain.GrainState>>();
        private readonly string id = Guid.NewGuid().ToString();
        private readonly TagGrain sut;

        public TagGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(typeof(TagGrain), id, A<HandleSnapshot<TagGrain.GrainState>>.Ignored))
                .Returns(persistence);

            sut = new TagGrain(store);
            sut.ActivateAsync(id).Wait();
        }

        [Fact]
        public async Task Should_delete_and_reset_state_when_cleaning()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("name2", "name3"), null);
            await sut.ClearAsync();

            var allTags = await sut.GetTagsAsync();

            Assert.Empty(allTags);

            A.CallTo(() => persistence.DeleteAsync())
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
