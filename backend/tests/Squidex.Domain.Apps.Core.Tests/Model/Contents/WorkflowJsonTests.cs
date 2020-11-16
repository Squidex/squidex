// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var workflow = new Workflow(
                Status.Draft, new Dictionary<Status, WorkflowStep>
                {
                    [Status.Draft] = new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Published] = WorkflowTransition.When("Expression", "Role1", "Role2")
                        },
                        "#00ff00",
                        NoUpdate.When("Expression", "Role1", "Role2"))
                }, new List<DomainId> { DomainId.NewGuid() }, "MyName");

            var serialized = workflow.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(workflow);
        }

        [Fact]
        public void Should_serialize_and_deserialize_default()
        {
            var workflow = Workflow.Default;

            var serialized = workflow.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(workflow);
        }

        [Fact]
        public void Should_deserialize_old_noUpdate_condition()
        {
            var jsonStep = new { noUpdate = true };

            var serialized = jsonStep.SerializeAndDeserialize<WorkflowStep>();

            serialized.Should().BeEquivalentTo(new WorkflowStep(null, null, NoUpdate.Always));
        }

        [Fact]
        public void Should_serialize_and_deserialize_no_update_condition()
        {
            var step = new WorkflowStep(noUpdate: NoUpdate.When("Expression", "Role1", "Role2"));

            var serialized = step.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(step);
        }

        [Fact]
        public void Should_verify_roles_mapping_in_workflow_transition()
        {
            var source = new JsonWorkflowTransition { Expression = "expression_1", Role = "role_1" };

            var serialized = source.SerializeAndDeserialize();

            var result = serialized.ToTransition();

            Assert.Equal(source.Role, result?.Roles?.Single());
        }
    }
}