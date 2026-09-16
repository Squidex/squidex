// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.States;
using IClock = NodaTime.IClock;
using TestUtils = Squidex.Domain.Apps.Core.TestHelpers.TestUtils;

namespace Squidex.Domain.Apps.Entities.Contents.Migration;

public class MigrateContentsJobTests : GivenContext
{
    private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
    private readonly List<BulkUpdateContents> commands = [];
    private readonly MigrateContentsJob sut;
    private Func<BulkUpdateContents, BulkUpdateResult> bulkUpdateResult = _ => new BulkUpdateResult();

    public MigrateContentsJobTests()
    {
        Schema =
            Schema
                .AddNumber(1, "my-field", Partitioning.Invariant)
                .AddString(2, "my-string", Partitioning.Invariant);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .ReturnsLazily(x =>
            {
                var command = x.GetArgument<ICommand>(0)!;

                if (command is BulkUpdateContents bulkUpdate)
                {
                    commands.Add(bulkUpdate);
                }

                return Task.FromResult(new CommandContext(command, commandBus).Complete(bulkUpdateResult((command as BulkUpdateContents)!)));
            });

        sut = new MigrateContentsJob(AppProvider, commandBus, contentRepository, contextProvider, TestUtils.DefaultSerializer);
    }

    [Fact]
    public void Should_create_request()
    {
        var job = MigrateContentsJob.BuildRequest(User, App, Schema, migrateDraft: false, migratePublished: true);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["migrateDraft"] = "False",
                ["migratePublished"] = "True",
            });
    }

    [Fact]
    public async Task Should_throw_exception_if_arguments_do_not_contain_schemaId()
    {
        var context = CreateRunContext(new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["schemaName"] = Schema.Name,
            }.ToReadonlyDictionary(),
        });

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_arguments_do_not_contain_schemaName()
    {
        var context = CreateRunContext(new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["schemaId"] = Schema.Id.ToString(),
            }.ToReadonlyDictionary(),
        });

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_schema_not_found()
    {
        A.CallTo(() => AppProvider.GetAppWithSchemaAsync(AppId.Id, SchemaId.Id, A<bool>._, A<CancellationToken>._))
            .Returns((App, null as Schema));

        var context = CreateRunContext(CreateJob());

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_not_submit_content_if_data_matches_schema()
    {
        SetupContents(CreateWrite(ValidData()));

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_submit_content_with_invalid_values()
    {
        var content = CreateWrite(InvalidData());

        SetupContents(content);

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        var command = Assert.Single(commands);

        Assert.Equal(SchemaId, command.SchemaId);

        var job = Assert.Single(command.Jobs!);

        Assert.Equal(content.Id, job.Id);
        Assert.Equal(BulkUpdateContentType.Migrate, job.Type);
        Assert.Equal(content.Version, job.ExpectedVersion);
        Assert.Null(job.NewData);

        // The invalid value has been removed.
        job.Data.Should().BeEquivalentTo(new ContentData());
    }

    [Fact]
    public async Task Should_submit_published_and_draft_data_for_content_with_draft()
    {
        SetupContents(CreateWrite(InvalidData(), draft: InvalidData()));

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        var job = Assert.Single(Assert.Single(commands).Jobs!);

        job.Data.Should().BeEquivalentTo(new ContentData());
        job.NewData.Should().BeEquivalentTo(new ContentData());
    }

    [Fact]
    public async Task Should_not_submit_content_with_draft_if_both_versions_match_schema()
    {
        SetupContents(CreateWrite(ValidData(), draft: ValidData()));

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_not_submit_published_content_if_published_versions_are_excluded()
    {
        SetupContents(CreateWrite(InvalidData()));

        await sut.RunAsync(CreateRunContext(CreateJob(migratePublished: false)), CancellationToken);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_not_submit_unpublished_content_if_draft_versions_are_excluded()
    {
        SetupContents(CreateWrite(InvalidData(), Status.Draft));

        await sut.RunAsync(CreateRunContext(CreateJob(migrateDraft: false)), CancellationToken);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_not_submit_draft_data_if_draft_versions_are_excluded()
    {
        SetupContents(CreateWrite(InvalidData(), draft: InvalidData()));

        await sut.RunAsync(CreateRunContext(CreateJob(migrateDraft: false)), CancellationToken);

        var job = Assert.Single(Assert.Single(commands).Jobs!);

        Assert.NotNull(job.Data);
        Assert.Null(job.NewData);
    }

    [Fact]
    public async Task Should_not_submit_published_data_if_published_versions_are_excluded()
    {
        SetupContents(CreateWrite(InvalidData(), draft: InvalidData()));

        await sut.RunAsync(CreateRunContext(CreateJob(migratePublished: false)), CancellationToken);

        var job = Assert.Single(Assert.Single(commands).Jobs!);

        Assert.Null(job.Data);
        Assert.NotNull(job.NewData);
    }

    [Fact]
    public async Task Should_migrate_all_versions_if_arguments_do_not_contain_flags()
    {
        SetupContents(CreateWrite(InvalidData(), draft: InvalidData()));

        var context = CreateRunContext(new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
            }.ToReadonlyDictionary(),
        });

        await sut.RunAsync(context, CancellationToken);

        var job = Assert.Single(Assert.Single(commands).Jobs!);

        Assert.NotNull(job.Data);
        Assert.NotNull(job.NewData);
    }

    [Fact]
    public async Task Should_migrate_content_again_if_version_does_not_match()
    {
        var content = CreateWrite(InvalidData());

        SetupContents(content);

        // The content has been changed after it has been read.
        var updated = content with { Version = content.Version + 1 };

        SetupConflicts(content, 1);
        SetupReload(updated);

        var context = CreateRunContext(CreateJob());

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal(2, commands.Count);
        Assert.Equal(updated.Version, Assert.Single(commands[1].Jobs!).ExpectedVersion);
        Assert.Equal("Checked contents: 1, submitted: 1", Assert.Single(context.Job.Log).Message);
    }

    [Fact]
    public async Task Should_log_error_if_version_never_matches()
    {
        var content = CreateWrite(InvalidData());

        SetupContents(content);
        SetupConflicts(content, int.MaxValue);
        SetupReload(content);

        var context = CreateRunContext(CreateJob());

        await sut.RunAsync(context, CancellationToken);

        // The first attempt and three retries.
        Assert.Equal(4, commands.Count);
        Assert.Contains(context.Job.Log, x => x.Message.StartsWith($"Failed to migrate content {content.Id}", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Should_submit_contents_in_batches()
    {
        SetupContents(Enumerable.Range(0, 150).Select(_ => CreateWrite(InvalidData())).ToArray());

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        Assert.Equal([100, 50], commands.Select(x => x.Jobs!.Length));
    }

    [Fact]
    public async Task Should_log_progress_as_single_line()
    {
        SetupContents(Enumerable.Range(0, 150).Select(_ => CreateWrite(InvalidData())).ToArray());

        var context = CreateRunContext(CreateJob());

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal("Checked contents: 150, submitted: 150", Assert.Single(context.Job.Log).Message);
    }

    [Fact]
    public async Task Should_not_overwrite_errors_with_progress()
    {
        var content = CreateWrite(InvalidData());

        SetupContents(content);

        bulkUpdateResult = _ => new BulkUpdateResult([new BulkUpdateResultItem(content.Id, 0, new DomainException("Error"))]);

        var context = CreateRunContext(CreateJob());

        await sut.RunAsync(context, CancellationToken);

        Assert.Equal(
            [
                $"Failed to migrate content {content.Id}: Error",
                "Checked contents: 1, submitted: 1",
            ],
            context.Job.Log.Select(x => x.Message));
    }

    private static ContentData ValidData()
    {
        return new ContentData()
            .AddField("my-field",
                new ContentFieldData()
                    .AddInvariant(42))
            .AddField("my-string",
                new ContentFieldData()
                    .AddInvariant("hello"));
    }

    private static ContentData InvalidData()
    {
        return new ContentData()
            .AddField("my-field",
                new ContentFieldData()
                    .AddInvariant("invalid"));
    }

    private void SetupContents(params WriteContent[] contents)
    {
        A.CallTo(() => contentRepository.StreamWriteContents(AppId.Id, A<HashSet<DomainId>>._, null, A<CancellationToken>._))
            .Returns(contents.ToAsyncEnumerable());
    }

    private void SetupReload(WriteContent content)
    {
        A.CallTo(() => contentRepository.StreamWriteContents(AppId.Id, A<HashSet<DomainId>>._, A<HashSet<DomainId>>.That.Not.IsNull(), A<CancellationToken>._))
            .Returns(Enumerable.Repeat(content, 1).ToAsyncEnumerable());
    }

    private void SetupConflicts(WriteContent content, int count)
    {
        var attempts = 0;

        bulkUpdateResult = _ =>
        {
            if (attempts++ >= count)
            {
                return new BulkUpdateResult();
            }

            return new BulkUpdateResult([new BulkUpdateResultItem(content.Id, 0, new DomainObjectVersionException(content.Id.ToString(), 1, 2))]);
        };
    }

    private WriteContent CreateWrite(ContentData data, Status? status = null, ContentData? draft = null)
    {
        var content = CreateWriteContent();

        return content with
        {
            CurrentVersion = new ContentVersion(status ?? Status.Published, data),
            NewVersion = draft != null ? new ContentVersion(Status.Draft, draft) : null,
        };
    }

    private Job CreateJob(bool migrateDraft = true, bool migratePublished = true)
    {
        return new Job
        {
            Arguments = MigrateContentsJob.BuildRequest(User, App, Schema, migrateDraft, migratePublished).Arguments,
        };
    }

    private JobRunContext CreateRunContext(Job job)
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        return new JobRunContext(state, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
