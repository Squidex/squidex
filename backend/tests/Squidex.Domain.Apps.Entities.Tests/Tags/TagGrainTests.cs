// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using Orleans.Core;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Xunit;

#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class TagGrainTests
    {
        private readonly IGrainIdentity identity = A.Fake<IGrainIdentity>();
        private readonly IGrainState<TagGrain.State> state = A.Fake<IGrainState<TagGrain.State>>();
        private readonly string id = DomainId.NewGuid().ToString();
        private readonly TagGrain sut;

        public TagGrainTests()
        {
            A.CallTo(() => identity.PrimaryKeyString)
                .Returns(id);

            A.CallTo(() => state.ClearAsync())
                .Invokes(() => state.Value = new TagGrain.State());

            sut = new TagGrain(identity, state);
        }

        [Fact]
        public async Task Should_delete_and_reset_state_if_cleaning()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("name1", "name2"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("name2", "name3"), null);
            await sut.ClearAsync();

            var allTags = await sut.GetTagsAsync();

            Assert.Empty(allTags);

            A.CallTo(() => state.ClearAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_rename_tag()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);

            await sut.RenameTagAsync("tag1", "tag1_new");

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1_new"), null);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1_new"] = 4
            }, allTags);
        }

        [Fact]
        public async Task Should_rename_tag_twice()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);

            await sut.RenameTagAsync("tag1", "tag1_new1");

            // Rename again.
            await sut.RenameTagAsync("tag1_new1", "tag1_new2");

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1_new1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1_new2"), null);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1_new2"] = 5
            }, allTags);
        }

        [Fact]
        public async Task Should_rename_tag_back()
        {
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);

            await sut.RenameTagAsync("tag1", "tag1_new1");

            // Rename back.
            await sut.RenameTagAsync("tag1_new1", "tag1");

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(HashSet.Of("tag1"), null);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1"] = 3
            }, allTags);
        }

        [Fact]
        public async Task Should_rebuild_tags()
        {
            var tags = new TagsExport
            {
                Tags = new Dictionary<string, Tag>
                {
                    ["id1"] = new Tag { Name = "name1", Count = 1 },
                    ["id2"] = new Tag { Name = "name2", Count = 2 },
                    ["id3"] = new Tag { Name = "name3", Count = 6 }
                }
            };

            await sut.RebuildAsync(tags);

            var allTags = await sut.GetTagsAsync();

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 2,
                ["name3"] = 6
            }, allTags);

            var export = await sut.GetExportableTagsAsync();

            export.Should().BeEquivalentTo(tags);
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
