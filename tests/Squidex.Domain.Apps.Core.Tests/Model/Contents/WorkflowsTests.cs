// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowsTests
    {
        private readonly Workflows workflows_0 = Workflows.Empty;

        [Fact]
        public void Should_provide_default_workflow_if_none_found()
        {
            var workflow = workflows_0.GetFirst();

            Assert.Same(Workflow.Default, workflow);
        }

        [Fact]
        public void Should_set_workflow_with_empty_guid()
        {
            var workflows_1 = workflows_0.Set(Workflow.Default);

            Assert.Single(workflows_1);
            Assert.Same(Workflow.Default, workflows_1[Guid.Empty]);
        }

        [Fact]
        public void Should_add_new_workflow_with_default_states()
        {
            var workflows_1 = workflows_0.Add("1");

            Assert.Equal(workflows_1.GetFirst().Steps.Keys, new[] { Status.Archived, Status.Draft, Status.Published });
        }

        [Fact]
        public void Should_update_workflow()
        {
            var workflows_1 = workflows_0.Add("1");
            var workflows_2 = workflows_1.Update(workflows_1.Keys.First(), Workflow.Empty);

            Assert.Empty(workflows_2.GetFirst().Steps.Keys);
        }

        [Fact]
        public void Should_do_nothing_if_workflow_to_update_not_found()
        {
            var workflows_1 = workflows_0.Update(Guid.NewGuid(), Workflow.Empty);

            Assert.Same(workflows_0, workflows_1);
        }

        [Fact]
        public void Should_remove_workflow()
        {
            var workflows_1 = workflows_0.Add("1");
            var workflows_2 = workflows_1.Remove(workflows_1.Keys.First());

            Assert.Empty(workflows_2);
        }

        [Fact]
        public void Should_do_nothing_if_workflow_to_remove_not_found()
        {
            var workflows_1 = workflows_0.Remove(Guid.NewGuid());

            Assert.Empty(workflows_1);
        }
    }
}
