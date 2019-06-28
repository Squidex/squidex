// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    }
}
