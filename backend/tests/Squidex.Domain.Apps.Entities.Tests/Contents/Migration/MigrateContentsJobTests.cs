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

                return Task.FromResult(new CommandContext(command, commandBus).Complete(new BulkUpdateResult()));
            });

        sut = new MigrateContentsJob(AppProvider, commandBus, contentRepository, contextProvider, TestUtils.DefaultSerializer);
    }

    [Fact]
    public void Should_create_request()
    {
        var job = MigrateContentsJob.BuildRequest(User, App, Schema);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
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
    public async Task Should_not_update_content_if_data_matches_schema()
    {
        SetupContents(CreateContent(new ContentData()
            .AddField("my-field",
                new ContentFieldData()
                    .AddInvariant(42))
            .AddField("my-string",
                new ContentFieldData()
                    .AddInvariant("hello"))));

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        Assert.Empty(commands);
    }

    [Fact]
    public async Task Should_update_content_with_invalid_values()
    {
        var content = CreateContent(new ContentData()
            .AddField("my-field",
                new ContentFieldData()
                    .AddInvariant("invalid"))
            .AddField("my-string",
                new ContentFieldData()
                    .AddInvariant("hello"))
            .AddField("removed-field",
                new ContentFieldData()
                    .AddInvariant(42)));

        SetupContents(content);

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        var command = Assert.Single(commands);

        Assert.Equal(SchemaId, command.SchemaId);
        Assert.True(command.DoNotScript);
        Assert.True(command.DoNotValidate);
        Assert.True(command.DoNotValidateWorkflow);

        var job = Assert.Single(command.Jobs!);

        Assert.Equal(content.Id, job.Id);
        Assert.Equal(BulkUpdateContentType.Update, job.Type);

        // The invalid value and the field that is not part of the schema anymore are removed.
        job.Data.Should().BeEquivalentTo(
            new ContentData()
                .AddField("my-string",
                    new ContentFieldData()
                        .AddInvariant("hello")));
    }

    [Fact]
    public async Task Should_update_contents_in_batches()
    {
        var contents = Enumerable.Range(0, 150).Select(_ => CreateContent(new ContentData()
            .AddField("my-field",
                new ContentFieldData()
                    .AddInvariant("invalid")))).ToArray();

        SetupContents(contents);

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        Assert.Equal([100, 50], commands.Select(x => x.Jobs!.Length));
    }

    private void SetupContents(params Content[] contents)
    {
        A.CallTo(() => contentRepository.StreamAll(AppId.Id, A<HashSet<DomainId>>._, SearchScope.All, A<CancellationToken>._))
            .Returns(contents.ToAsyncEnumerable());
    }

    private Content CreateContent(ContentData data)
    {
        var content = CreateContent();

        content.Data = data;

        return content;
    }

    private Job CreateJob()
    {
        return new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
            }.ToReadonlyDictionary(),
        };
    }

    private JobRunContext CreateRunContext(Job job)
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        return new JobRunContext(state, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
