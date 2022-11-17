// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class WorkflowJsonTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var workflow = new Workflow(
            Status.Draft,
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Draft] = new WorkflowStep(
                    new Dictionary<Status, WorkflowTransition>
                    {
                        [Status.Published] = WorkflowTransition.When("Expression", "Role1", "Role2")
                    }.ToReadonlyDictionary(),
                    "#00ff00",
                    NoUpdate.When("Expression", "Role1", "Role2"),
                    true)
            }.ToReadonlyDictionary(),
            ReadonlyList.Create(DomainId.NewGuid()), "MyName");

        var serialized = workflow.SerializeAndDeserialize();

        Assert.Equal(workflow, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_default()
    {
        var workflow = Workflow.Default;

        var serialized = workflow.SerializeAndDeserialize();

        Assert.Equal(workflow, serialized);
    }

    [Fact]
    public void Should_deserialize_old_noUpdate_condition()
    {
        var jsonStep = new { noUpdate = true };

        var serialized = jsonStep.SerializeAndDeserialize<WorkflowStep, object>();

        Assert.Equal(new WorkflowStep(null, null, NoUpdate.Always), serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_no_update_condition()
    {
        var step = new WorkflowStep(NoUpdate: NoUpdate.When("Expression", "Role1", "Role2"));

        var serialized = step.SerializeAndDeserialize();

        Assert.Equal(step, serialized);
    }

    [Fact]
    public void Should_verify_roles_mapping_in_workflow_transition()
    {
        var source = new WorkflowTransitionSurrogate { Expression = "expression_1", Role = "role_1" };

        var serialized = source.SerializeAndDeserialize();

        var actual = serialized.ToSource();

        Assert.Equal(source.Role, actual?.Roles?.Single());
    }
}
