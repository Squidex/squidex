// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Tags;

public class TagServiceTests : GivenContext
{
    private readonly TestState<TagService.State> state;
    private readonly string group = DomainId.NewGuid().ToString();
    private readonly string stateId;
    private readonly TagService sut;

    public TagServiceTests()
    {
        stateId = $"{AppId.Id}_{group}";
        state = new TestState<TagService.State>(stateId);

        sut = new TagService(state.PersistenceFactory);
    }

    [Fact]
    public async Task Should_delete_and_reset_state_if_cleaning()
    {
        await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);
        await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag2", "tag3"), CancellationToken);

        await sut.ClearAsync(AppId.Id, group, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Empty(allTags);

        A.CallTo(() => state.Persistence.DeleteAsync(CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_unset_count_on_full_clear()
    {
        var ids = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag1"]] = 1,
            [ids["tag2"]] = 1
        }, CancellationToken);

        // Clear is called by the event consumer to fill the counts again, therefore we do not delete other things.
        await sut.ClearAsync(CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 0,
            ["tag2"] = 0
        }, allTags);

        A.CallTo(() => state.Persistence.DeleteAsync(CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_rename_tag()
    {
        var ids_0 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);

        await sut.RenameTagAsync(AppId.Id, group, "tag_0", "tag_1", CancellationToken);

        // Both names should map to the same tag.
        var ids_1 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);
        var ids_2 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_1"), CancellationToken);

        Assert.Equal(ids_0.Values, ids_1.Values);
        Assert.Equal(ids_0.Values, ids_2.Values);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag_1"] = 0
        }, allTags);
    }

    [Fact]
    public async Task Should_rename_tag_twice()
    {
        var ids_0 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);

        // Forward the old name to the new name.
        await sut.RenameTagAsync(AppId.Id, group, "tag_0", "tag_1", CancellationToken);
        await sut.RenameTagAsync(AppId.Id, group, "tag_1", "tag_2", CancellationToken);

        // All names should map to the same tag.
        var ids_1 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);
        var ids_2 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_1"), CancellationToken);
        var ids_3 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_2"), CancellationToken);

        Assert.Equal(ids_0.Values, ids_1.Values);
        Assert.Equal(ids_0.Values, ids_2.Values);
        Assert.Equal(ids_0.Values, ids_3.Values);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag_2"] = 0
        }, allTags);
    }

    [Fact]
    public async Task Should_rename_tag_back()
    {
        var ids_0 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);

        // Forward the old name to the new name.
        await sut.RenameTagAsync(AppId.Id, group, "tag_0", "tag_1", CancellationToken);
        await sut.RenameTagAsync(AppId.Id, group, "tag_1", "tag_0", CancellationToken);

        // All names should map to the same tag.
        var ids_1 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_0"), CancellationToken);
        var ids_2 = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag_1"), CancellationToken);

        Assert.Equal(ids_0.Values, ids_1.Values);
        Assert.Equal(ids_0.Values, ids_2.Values);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag_0"] = 0
        }, allTags);
    }

    [Fact]
    public async Task Should_merge_tags_on_rename()
    {
        var ids = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag1"]] = 1,
            [ids["tag2"]] = 2
        }, CancellationToken);

        await sut.RenameTagAsync(AppId.Id, group, "tag2", "tag1", CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 3
        }, allTags);
    }

    [Fact]
    public async Task Should_merge_tags_when_stored_with_duplicate_names()
    {
        var tags = new TagsExport
        {
            Tags = new Dictionary<string, Tag>
            {
                ["id1"] = new Tag { Name = "tag1", Count = 10 },
                ["id2"] = new Tag { Name = "tag1", Count = 20 }
            },
            Alias = null!
        };

        await sut.RebuildTagsAsync(AppId.Id, group, tags, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 30
        }, allTags);
    }

    [Fact]
    public async Task Should_fix_names_when_stored_with_wrong_names()
    {
        var tags = new TagsExport
        {
            Tags = new Dictionary<string, Tag>
            {
                ["id1"] = new Tag { Name = "tag1 ", Count = 10 },
                ["id2"] = new Tag { Name = "tag2,", Count = 20 },
                ["id3"] = new Tag { Name = " tag3,", Count = 30 },
                ["id4"] = new Tag { Name = ",tag4,", Count = 40 }
            },
            Alias = null!
        };

        await sut.RebuildTagsAsync(AppId.Id, group, tags, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 10,
            ["tag2"] = 20,
            ["tag3"] = 30,
            ["tag4"] = 40
        }, allTags);
    }

    [Fact]
    public async Task Should_rebuild_tags()
    {
        var tags = new TagsExport
        {
            Tags = new Dictionary<string, Tag>
            {
                ["id1"] = new Tag { Name = "tag1", Count = 1 },
                ["id2"] = new Tag { Name = "tag2", Count = 2 },
                ["id3"] = new Tag { Name = "tag3", Count = 6 }
            },
            Alias = null!
        };

        await sut.RebuildTagsAsync(AppId.Id, group, tags, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 1,
            ["tag2"] = 2,
            ["tag3"] = 6
        }, allTags);

        var export = await sut.GetExportableTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(tags.Tags, export.Tags);
        Assert.Empty(export.Alias);
    }

    [Fact]
    public async Task Should_rebuild_with_broken_export()
    {
        var tags = new TagsExport
        {
            Alias = new Dictionary<string, string>
            {
                ["id1"] = "id2"
            },
            Tags = null!
        };

        await sut.RebuildTagsAsync(AppId.Id, group, tags, CancellationToken);

        var export = await sut.GetExportableTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(tags.Alias, export.Alias);
        Assert.Empty(export.Tags);
    }

    [Fact]
    public async Task Should_add_tag_but_not_count_tags()
    {
        await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);
        await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag2", "tag3"), CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 0,
            ["tag2"] = 0,
            ["tag3"] = 0
        }, allTags);
    }

    [Fact]
    public async Task Should_add_and_increment_tags()
    {
        var ids = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2", "tag3"), CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag1"]] = 1,
            [ids["tag2"]] = 1
        }, CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag2"]] = 1,
            [ids["tag3"]] = 1
        }, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 1,
            ["tag2"] = 2,
            ["tag3"] = 1
        }, allTags);
    }

    [Fact]
    public async Task Should_add_and_decrement_tags()
    {
        var ids = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2", "tag3"), CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag1"]] = 1,
            [ids["tag2"]] = 1
        }, CancellationToken);

        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            [ids["tag2"]] = -2,
            [ids["tag3"]] = -2
        }, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Equal(new Dictionary<string, int>
        {
            ["tag1"] = 1,
            ["tag2"] = 0,
            ["tag3"] = 0
        }, allTags);
    }

    [Fact]
    public async Task Should_not_update_non_existing_tags()
    {
        // We have no names for these IDs so we cannot update it.
        await sut.UpdateAsync(AppId.Id, group, new Dictionary<string, int>
        {
            ["id1"] = 1,
            ["id2"] = 1
        }, CancellationToken);

        var allTags = await sut.GetTagsAsync(AppId.Id, group, CancellationToken);

        Assert.Empty(allTags);
    }

    [Fact]
    public async Task Should_resolve_tag_names()
    {
        // Get IDs from names.
        var tagIds = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);

        // Get names from IDs (reverse operation).
        var tagNames = await sut.GetTagNamesAsync(AppId.Id, group, tagIds.Values.ToHashSet(), CancellationToken);

        Assert.Equal(tagIds.Keys.ToArray(), tagNames.Values.ToArray());
    }

    [Fact]
    public async Task Should_get_exportable_tags()
    {
        var ids = await sut.GetTagIdsAsync(AppId.Id, group, HashSet.Of("tag1", "tag2"), CancellationToken);

        var allTags = await sut.GetExportableTagsAsync(AppId.Id, group, CancellationToken);

        allTags.Tags.Should().BeEquivalentTo(new Dictionary<string, Tag>
        {
            [ids["tag1"]] = new Tag { Name = "tag1", Count = 0 },
            [ids["tag2"]] = new Tag { Name = "tag2", Count = 0 },
        });
    }
}
