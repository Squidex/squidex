// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowsJsonTests
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
                }, new List<Guid> { Guid.NewGuid() }, "MyName");

            var serialized = workflow.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(workflow);
        }
    }
}
