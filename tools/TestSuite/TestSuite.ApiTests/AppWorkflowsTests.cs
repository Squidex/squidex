// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class AppWorkflowsTests : IClassFixture<ClientFixture>
{
    private readonly string workflowName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public AppWorkflowsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_workflow()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Create workflow.
        var workflow = await CreateAsync(app);

        Assert.NotNull(workflow);
        Assert.NotNull(workflow.Name);
        Assert.Equal(3, workflow.Steps.Count);

        await Verify(workflow);
    }

    [Fact]
    public async Task Should_update_workflow()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 0: Create workflow.
        var workflow = await CreateAsync(app);


        // STEP 1: Update workflow.
        var updateRequest = new UpdateWorkflowDto
        {
            Initial = "Draft",
            Steps = new Dictionary<string, WorkflowStepDto>
            {
                ["Draft"] = new WorkflowStepDto
                {
                    Transitions = new Dictionary<string, WorkflowTransitionDto>
                    {
                        ["Published"] = new WorkflowTransitionDto()
                    }
                },
                ["Published"] = new WorkflowStepDto(),
            },
            Name = workflowName
        };

        var workflows_2 = await app.Apps.PutWorkflowAsync(workflow.Id, updateRequest);
        var workflow_2 = workflows_2.Items.Find(x => x.Name == workflowName);

        Assert.NotNull(workflow_2);
        Assert.NotNull(workflow_2.Name);
        Assert.Equal(2, workflow_2.Steps.Count);

        await Verify(workflows_2);
    }

    [Fact]
    public async Task Should_delete_workflow()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 0: Create workflow.
        var workflow = await CreateAsync(app);


        // STEP 1: Delete workflow.
        var workflows_2 = await app.Apps.DeleteWorkflowAsync(workflow.Id);

        Assert.DoesNotContain(workflows_2.Items, x => x.Name == workflowName);

        await Verify(workflows_2);
    }

    private async Task<WorkflowDto> CreateAsync(ISquidexClient app)
    {
        var createRequest = new AddWorkflowDto
        {
            Name = workflowName
        };

        var workflows = await app.Apps.PostWorkflowAsync(createRequest);
        var workflow = workflows.Items.Find(x => x.Name == workflowName);

        return workflow!;
    }
}
