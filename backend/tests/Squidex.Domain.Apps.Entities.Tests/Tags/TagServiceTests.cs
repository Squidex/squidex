// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace Squidex.Domain.Apps.Entities.Tags
{
    public class TagServiceTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly TestState<TagService.State> state;
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly string group = DomainId.NewGuid().ToString();
        private readonly string stateId;
        private readonly TagService sut;

        public TagServiceTests()
        {
            ct = cts.Token;

            stateId = $"{appId}_{group}";
            state = new TestState<TagService.State>(stateId);

            sut = new TagService(state.PersistenceFactory);
        }

        [Fact]
        public async Task Should_delete_and_reset_state_if_cleaning()
        {
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name2", "name3"), null, ct);
            await sut.ClearAsync(appId, group, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Empty(allTags);

            A.CallTo(() => state.Persistence.DeleteAsync(ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_rename_tag()
        {
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);

            await sut.RenameTagAsync(appId, group, "tag1", "tag1_new", ct);

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1_new"), null, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1_new"] = 4
            }, allTags);
        }

        [Fact]
        public async Task Should_rename_tag_twice()
        {
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);

            await sut.RenameTagAsync(appId, group, "tag1", "tag1_new1", ct);

            // Rename again.
            await sut.RenameTagAsync(appId, group, "tag1_new1", "tag1_new2", ct);

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1_new1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1_new2"), null, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Equal(new Dictionary<string, int>
            {
                ["tag1_new2"] = 5
            }, allTags);
        }

        [Fact]
        public async Task Should_rename_tag_back()
        {
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);

            await sut.RenameTagAsync(appId, group, "tag1", "tag1_new1", ct);

            // Rename back.
            await sut.RenameTagAsync(appId, group, "tag1_new1", "tag1", ct);

            // Forward the old name to the new name.
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("tag1"), null, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

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

            await sut.RebuildTagsAsync(appId, group, tags, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 2,
                ["name3"] = 6
            }, allTags);

            var export = await sut.GetExportableTagsAsync(appId, group, ct);

            export.Should().BeEquivalentTo(tags);
        }

        [Fact]
        public async Task Should_add_tags()
        {
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2"), null, ct);
            await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name2", "name3"), null, ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

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
            var result1 = await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2"), null, ct);
            var result2 = await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2", "name3"), new HashSet<string>(result1.Values), ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Equal(new Dictionary<string, int>
            {
                ["name1"] = 1,
                ["name2"] = 1,
                ["name3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_remove_tags)
        {
            var result1 = await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2"), null, ct);
            var result2 = await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name2", "name3"), null, ct);

            await sut.NormalizeTagsAsync(appId, group, null, new HashSet<string>(result1.Values), ct);

            var allTags = await sut.GetTagsAsync(appId, group, ct);

            Assert.Equal(new Dictionary<string, int>
            {
                ["name2"] = 1,
                ["name3"] = 1
            }, allTags);
        }

        [Fact]
        public async Task Should_resolve_tag_names()
        {
            var tagIds = await sut.NormalizeTagsAsync(appId, group, HashSet.Of("name1", "name2"), null, ct);

            // Just the inverse operation of the normalization.
            var tagNames = await sut.GetTagIdsAsync(appId, group, HashSet.Of("name1", "name2", "invalid1"), ct);

            Assert.Equal(tagIds, tagNames);
        }
    }
}
