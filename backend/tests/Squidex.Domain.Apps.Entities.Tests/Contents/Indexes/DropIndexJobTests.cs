// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions.Common;
using Jint.Runtime;
using NodaTime;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using IClock = NodaTime.IClock;

namespace Squidex.Domain.Apps.Entities.Contents.Indexes;

public class DropIndexJobTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly DropIndexJob sut;

    public DropIndexJobTests()
    {
        sut = new DropIndexJob(contentRepository);
    }

    [Fact]
    public void Should_create_request()
    {
        var job = DropIndexJob.BuildRequest(User, App, Schema, "MyIndex");

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["indexName"] = "MyIndex"
            });
    }

    [Fact]
    public async Task Should_throw_exception_if_arguments_do_not_contain_schemaId()
    {
        var job = new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaName"] = Schema.Name,
                ["indexName"] = "MyIndex"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_arguments_do_not_contain_schemaName()
    {
        var job = new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["indexName"] = "MyIndex"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_arguments_do_not_contain_index_name()
    {
        var job = new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_invoke_content_repository()
    {
        var job = new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["indexName"] = "MyIndex"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await sut.RunAsync(context, CancellationToken);

        A.CallTo(() => contentRepository.DropIndexAsync(App.Id, Schema.Id, "MyIndex", CancellationToken))
            .MustHaveHappened();
    }

    private JobRunContext CreateContext(Job job)
    {
        return new JobRunContext(null!, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
