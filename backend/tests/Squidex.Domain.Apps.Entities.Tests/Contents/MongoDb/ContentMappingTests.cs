// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public class ContentMappingTests : GivenContext
{
    [Fact]
    public async Task Should_map_content_without_new_version_to_draft()
    {
        var source = CreateWriteContent();

        var snapshotJob = new SnapshotWriteJob<WriteContent>(source.UniqueId, source, source.Version);
        var snapshot = await MongoContentEntity.CreateCompleteAsync(snapshotJob, AppProvider, default);

        Assert.Equal(source.CurrentVersion.Data, snapshot.Data);
        Assert.Null(snapshot.NewData);
        Assert.Null(snapshot.NewStatus);
        Assert.NotNull(snapshot.ScheduleJob);
        Assert.True(snapshot.IsSnapshot);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }

    [Fact]
    public async Task Should_map_content_without_new_version_to_published()
    {
        var source = CreateWriteContent();

        var snapshotJob = new SnapshotWriteJob<WriteContent>(source.UniqueId, source, source.Version);
        var snapshot = await MongoContentEntity.CreatePublishedAsync(snapshotJob, AppProvider, default);

        Assert.Equal(source.CurrentVersion.Data, snapshot.Data);
        Assert.Null(snapshot.NewData);
        Assert.Null(snapshot.NewStatus);
        Assert.Null(snapshot.ScheduleJob);
        Assert.False(snapshot.IsSnapshot);
    }

    [Fact]
    public async Task Should_map_content_with_new_version_to_draft()
    {
        var source = CreateContentWithNewVersion();

        var snapshotJob = new SnapshotWriteJob<WriteContent>(source.UniqueId, source, source.Version);
        var snapshot = await MongoContentEntity.CreateCompleteAsync(snapshotJob, AppProvider, default);

        Assert.Equal(source.NewVersion?.Data, snapshot.Data);
        Assert.Equal(source.CurrentVersion.Data, snapshot.NewData);
        Assert.NotNull(snapshot.NewStatus);
        Assert.NotNull(snapshot.ScheduleJob);
        Assert.True(snapshot.IsSnapshot);

        var mapped = snapshot.ToState();

        mapped.Should().BeEquivalentTo(source);
    }

    [Fact]
    public async Task Should_map_content_with_new_version_to_published()
    {
        var source = CreateContentWithNewVersion();

        var snapshotJob = new SnapshotWriteJob<WriteContent>(source.UniqueId, source, source.Version);
        var snapshot = await MongoContentEntity.CreatePublishedAsync(snapshotJob, AppProvider, default);

        Assert.Equal(source.CurrentVersion?.Data, snapshot.Data);
        Assert.Null(snapshot.NewData);
        Assert.Null(snapshot.NewStatus);
        Assert.Null(snapshot.ScheduleJob);
        Assert.False(snapshot.IsSnapshot);
    }

    private WriteContent CreateContentWithNewVersion()
    {
        return CreateWriteContent() with
        {
            NewVersion =
                new ContentVersion(
                    Status.Draft,
                    new ContentData()
                        .AddField("my-field",
                            new ContentFieldData()
                                .AddInvariant(13))),
        };
    }
}
