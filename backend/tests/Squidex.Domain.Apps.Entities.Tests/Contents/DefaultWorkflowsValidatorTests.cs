// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Entities.Contents;

public class DefaultWorkflowsValidatorTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly DefaultWorkflowsValidator sut;

    public DefaultWorkflowsValidatorTests()
    {
        sut = new DefaultWorkflowsValidator(AppProvider);
    }

    [Fact]
    public async Task Should_generate_error_if_multiple_workflows_cover_all_schemas()
    {
        var workflows = Workflows.Empty
            .Add(DomainId.NewGuid(), "workflow1")
            .Add(DomainId.NewGuid(), "workflow2");

        var errors = await sut.ValidateAsync(AppId.Id, workflows);

        Assert.Equal(new[] { "Multiple workflows cover all schemas." }, errors.ToArray());
    }

    [Fact]
    public async Task Should_generate_error_if_multiple_workflows_cover_specific_schema()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var workflows = Workflows.Empty
            .Add(id1, "workflow1")
            .Add(id2, "workflow2")
            .Update(id1, new Workflow(default, null, ReadonlyList.Create(SchemaId.Id)))
            .Update(id2, new Workflow(default, null, ReadonlyList.Create(SchemaId.Id)));

        var errors = await sut.ValidateAsync(AppId.Id, workflows);

        Assert.Equal(new[] { "The schema 'my-schema' is covered by multiple workflows." }, errors.ToArray());
    }

    [Fact]
    public async Task Should_not_generate_error_if_schema_deleted()
    {
        Schema = null!;

        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var workflows = Workflows.Empty
            .Add(id1, "workflow1")
            .Add(id2, "workflow2")
            .Update(id1, new Workflow(default, null, ReadonlyList.Create(SchemaId.Id)))
            .Update(id2, new Workflow(default, null, ReadonlyList.Create(SchemaId.Id)));

        var errors = await sut.ValidateAsync(AppId.Id, workflows);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_generate_errors_for_no_overlaps()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var workflows = Workflows.Empty
            .Add(id1, "workflow1")
            .Add(id2, "workflow2")
            .Update(id1, new Workflow(default, null, ReadonlyList.Create(SchemaId.Id)));

        var errors = await sut.ValidateAsync(AppId.Id, workflows);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Should_not_generate_errors_for_empty_workflows()
    {
        var errors = await sut.ValidateAsync(AppId.Id, Workflows.Empty);

        Assert.Empty(errors);
    }
}
