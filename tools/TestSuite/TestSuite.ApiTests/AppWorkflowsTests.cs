// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public sealed class AppWorkflowsTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string name = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public AppWorkflowsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_workflow()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Create workflow.
        var workflow = await CreateAsync();

        Assert.NotNull(workflow);
        Assert.NotNull(workflow.Name);
        Assert.Equal(3, workflow.Steps.Count);

        await Verify(workflow);
    }

    [Fact]
    public async Task Should_update_workflow()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 0: Create workflow.
        var workflow = await CreateAsync();


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
            Name = name
        };

        var workflows_2 = await _.Apps.PutWorkflowAsync(appName, workflow.Id, updateRequest);
        var workflow_2 = workflows_2.Items.Find(x => x.Name == name);

        Assert.NotNull(workflow_2);
        Assert.NotNull(workflow_2.Name);
        Assert.Equal(2, workflow_2.Steps.Count);

        await Verify(workflows_2);
    }

    [Fact]
    public async Task Should_delete_workflow()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 0: Create workflow.
        var workflow = await CreateAsync();


        // STEP 1: Delete workflow.
        var workflows_2 = await _.Apps.DeleteWorkflowAsync(appName, workflow.Id);

        Assert.DoesNotContain(workflows_2.Items, x => x.Name == name);

        await Verify(workflows_2);
    }

    private async Task<WorkflowDto> CreateAsync()
    {
        var createRequest = new AddWorkflowDto
        {
            Name = name
        };

        var workflows = await _.Apps.PostWorkflowAsync(appName, createRequest);
        var workflow = workflows.Items.Find(x => x.Name == name);

        return workflow;
    }

    private async Task CreateAppAsync()
    {
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        await _.Apps.PostAppAsync(createRequest);
    }
}
