// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public class DefaultContentWorkflowTests
{
    private readonly DefaultContentWorkflow sut = new DefaultContentWorkflow();

    [Fact]
    public async Task Should_return_info_for_valid_status()
    {
        var info = await sut.GetInfoAsync(null!, Status.Draft);

        Assert.Equal(new StatusInfo(Status.Draft, StatusColors.Draft), info);
    }

    [Fact]
    public async Task Should_return_info_as_null_for_invalid_status()
    {
        var info = await sut.GetInfoAsync(null!, new Status("Invalid"));

        Assert.Null(info);
    }

    [Fact]
    public async Task Should_return_draft_as_initial_status()
    {
        var actual = await sut.GetInitialStatusAsync(null!);

        Assert.Equal(Status.Draft, actual);
    }

    [Fact]
    public async Task Should_allow_publish_on_create()
    {
        var actual = await sut.CanPublishInitialAsync(null!, null);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_allow_if_transition_is_valid()
    {
        var content = new ContentEntity { Status = Status.Published };

        var actual = await sut.CanMoveToAsync(null!, content.Status, Status.Draft, null!, null!);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_allow_if_transition_is_valid_for_content()
    {
        var content = new ContentEntity { Status = Status.Published };

        var actual = await sut.CanMoveToAsync(content, content.Status, Status.Draft, null!);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_published()
    {
        var content = new ContentEntity { Status = Status.Published };

        var actual = await sut.CanUpdateAsync(content, content.Status, null!);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_be_able_to_update_draft()
    {
        var content = new ContentEntity { Status = Status.Published };

        var actual = await sut.CanUpdateAsync(content, content.Status, null!);

        Assert.True(actual);
    }

    [Fact]
    public async Task Should_not_be_able_to_update_archived()
    {
        var content = new ContentEntity { Status = Status.Archived };

        var actual = await sut.CanUpdateAsync(content, content.Status, null!);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_draft()
    {
        var content = new ContentEntity { Status = Status.Draft };

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetNextAsync(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_archived()
    {
        var content = new ContentEntity { Status = Status.Archived };

        var expected = new[]
        {
            new StatusInfo(Status.Draft, StatusColors.Draft)
        };

        var actual = await sut.GetNextAsync(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_next_statuses_for_published()
    {
        var content = new ContentEntity { Status = Status.Published };

        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft)
        };

        var actual = await sut.GetNextAsync(content, content.Status, null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_return_all_statuses()
    {
        var expected = new[]
        {
            new StatusInfo(Status.Archived, StatusColors.Archived),
            new StatusInfo(Status.Draft, StatusColors.Draft),
            new StatusInfo(Status.Published, StatusColors.Published)
        };

        var actual = await sut.GetAllAsync(null!);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_not_validate_when_not_publishing()
    {
        var actual = await sut.ShouldValidateAsync(null!, Status.Draft);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_not_validate_when_publishing_but_not_enabled()
    {
        var actual = await sut.ShouldValidateAsync(CreateSchema(false), Status.Published);

        Assert.False(actual);
    }

    [Fact]
    public async Task Should_validate_when_publishing_and_enabled()
    {
        var actual = await sut.ShouldValidateAsync(CreateSchema(true), Status.Published);

        Assert.True(actual);
    }

    private static ISchemaEntity CreateSchema(bool validateOnPublish)
    {
        var schema = new Schema("my-schema", new SchemaProperties
        {
            ValidateOnPublish = validateOnPublish
        });

        return Mocks.Schema(NamedId.Of(DomainId.NewGuid(), "my-app"), NamedId.Of(DomainId.NewGuid(), schema.Name), schema);
    }
}
