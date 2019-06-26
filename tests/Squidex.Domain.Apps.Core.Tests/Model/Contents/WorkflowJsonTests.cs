// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
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
        public void Should_serialize_and_deserialize_workflows()
        {
            var workflow = Workflows.Empty.Set(Workflow.Default);

            var serialized = workflow.SerializeAndDeserialize();

            serialized.Should().BeEquivalentTo(workflow);
        }
    }
}
