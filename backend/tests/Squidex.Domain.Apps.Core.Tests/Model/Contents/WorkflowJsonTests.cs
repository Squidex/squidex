﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowJsonTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var workflow = Workflow.Default;

            var serialized = workflow.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(workflow);
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

            Assert.Equal(source.Role, result?.Roles.Single());
        }
    }
}