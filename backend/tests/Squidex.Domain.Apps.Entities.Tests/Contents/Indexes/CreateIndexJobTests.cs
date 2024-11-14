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
using Squidex.Infrastructure.TestHelpers;
using System.Security.Principal;
using IClock = NodaTime.IClock;

namespace Squidex.Domain.Apps.Entities.Contents.Indexes;

public class CreateIndexJobTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly CreateIndexJob sut;

    public CreateIndexJobTests()
    {
        sut = new CreateIndexJob(contentRepository);
    }

    [Fact]
    public void Should_create_request()
    {
        var job =
            CreateIndexJob.BuildRequest(User, App, Schema,
                [
                    new IndexField("field1", SortOrder.Ascending),
                    new IndexField("field2", SortOrder.Descending),
                ]);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["field_field1"] = "Ascending",
                ["field_field2"] = "Descending"
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
                ["field_field1"] = "Ascending",
                ["field_field2"] = "Descending"
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
                ["field_field1"] = "Ascending",
                ["field_field2"] = "Descending"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_field_order_is_invalid()
    {
        var job = new Job
        {
            Arguments = new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
                ["field_field1"] = "Invalid",
                ["field_field2"] = "Descending"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_fields_are_empty()
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
                ["field_field1"] = "Ascending",
                ["field_field2"] = "Descending"
            }.ToReadonlyDictionary()
        };

        var context = CreateContext(job);

        IndexDefinition? index = null;

        A.CallTo(() => contentRepository.CreateIndexAsync(App.Id, Schema.Id, A<IndexDefinition>._, CancellationToken))
            .Invokes(x => index = x.GetArgument<IndexDefinition>(2));

        await sut.RunAsync(context, CancellationToken);

        index.Should().BeEquivalentTo(
            [
                new IndexField("field1", SortOrder.Ascending),
                new IndexField("field2", SortOrder.Descending)
            ]);
    }

    private JobRunContext CreateContext(Job job)
    {
        return new JobRunContext(null!, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
