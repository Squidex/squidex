// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowTests
    {
        private readonly Workflow workflow = new Workflow(
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = new WorkflowTransition("ToArchivedExpr", "ToArchivedRole"),
                            [Status.Published] = new WorkflowTransition("ToPublishedExpr", "ToPublishedRole")
                        },
                        StatusColors.Draft),
                [Status.Archived] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>(),
                        StatusColors.Archived, true),
                [Status.Published] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>(),
                        StatusColors.Archived)
            }, Status.Draft);

        [Fact]
        public void Should_provide_initial_state()
        {
            var (status, step) = workflow.GetInitialStep();

            Assert.Equal(Status.Draft, status);
            Assert.Equal(StatusColors.Draft, step.Color);
            Assert.Same(workflow.Steps[Status.Draft], step);
        }

        [Fact]
        public void Should_provide_step()
        {
            var found = workflow.TryGetStep(Status.Draft, out var step);

            Assert.True(found);
            Assert.Same(workflow.Steps[Status.Draft], step);
        }

        [Fact]
        public void Should_not_provide_unknown_step()
        {
            var found = workflow.TryGetStep(default, out var step);

            Assert.False(found);
            Assert.Null(step);
        }

        [Fact]
        public void Should_provide_transition()
        {
            var found = workflow.TryGetTransition(Status.Draft, Status.Archived, out var transition);

            Assert.True(found);
            Assert.Equal("ToArchivedExpr", transition.Expression);
            Assert.Equal("ToArchivedRole", transition.Role);
        }

        [Fact]
        public void Should_not_provide_transition_from_unknown_step()
        {
            var found = workflow.TryGetTransition(default, Status.Archived, out var transition);

            Assert.False(found);
            Assert.Null(transition);
        }

        [Fact]
        public void Should_not_provide_transition_to_unknown_step()
        {
            var found = workflow.TryGetTransition(Status.Draft, default, out var transition);

            Assert.False(found);
            Assert.Null(transition);
        }

        [Fact]
        public void Should_provide_transitions()
        {
            var transitions = workflow.GetTransitions(Status.Draft).ToArray();

            Assert.Equal(2, transitions.Length);

            var (status1, step1, transition1) = transitions[0];

            Assert.Equal(Status.Archived, status1);
            Assert.Equal("ToArchivedExpr", transition1.Expression);
            Assert.Equal("ToArchivedRole", transition1.Role);
            Assert.Same(workflow.Steps[Status.Archived], step1);

            var (status2, step2, transition2) = transitions[1];

            Assert.Equal(Status.Published, status2);
            Assert.Equal("ToPublishedExpr", transition2.Expression);
            Assert.Equal("ToPublishedRole", transition2.Role);
            Assert.Same(workflow.Steps[Status.Published], step2);
        }
    }
}
