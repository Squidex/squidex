// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Collections;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class WorkflowTests
    {
        private readonly Workflow workflow = new Workflow(
            Status.Draft,
            new Dictionary<Status, WorkflowStep>
            {
                [Status.Draft] =
                    new WorkflowStep(
                        new Dictionary<Status, WorkflowTransition>
                        {
                            [Status.Archived] = WorkflowTransition.When("ToArchivedExpr", "ToArchivedRole"),
                            [Status.Published] = WorkflowTransition.When("ToPublishedExpr", "ToPublishedRole")
                        }.ToImmutableDictionary(),
                        StatusColors.Draft),
                [Status.Archived] =
                    new WorkflowStep(),
                [Status.Published] =
                    new WorkflowStep()
            }.ToImmutableDictionary());

        [Fact]
        public void Should_provide_default_workflow_if_none_found()
        {
            var result = Workflows.Empty.GetFirst();

            Assert.Same(Workflow.Default, result);
        }

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
            Assert.Equal("ToArchivedExpr", transition?.Expression);
            Assert.Equal("ToArchivedRole", transition?.Roles?.Single());
        }

        [Fact]
        public void Should_provide_transition_to_initial_if_step_not_found()
        {
            var found = workflow.TryGetTransition(new Status("Other"), Status.Draft, out var transition);

            Assert.True(found);
            Assert.Null(transition?.Expression);
            Assert.Null(transition?.Roles);
        }

        [Fact]
        public void Should_not_provide_transition_from_unknown_step()
        {
            var found = workflow.TryGetTransition(new Status("Other"), Status.Archived, out var transition);

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
            Assert.Equal("ToArchivedExpr", transition1?.Expression);
            Assert.Equal("ToArchivedRole", transition1?.Roles?.Single());
            Assert.Same(workflow.Steps[status1], step1);

            var (status2, step2, transition2) = transitions[1];

            Assert.Equal(Status.Published, status2);
            Assert.Equal("ToPublishedExpr", transition2?.Expression);
            Assert.Equal("ToPublishedRole", transition2?.Roles?.Single());
            Assert.Same(workflow.Steps[status2], step2);
        }

        [Fact]
        public void Should_provide_transitions_to_initial_step_if_status_not_found()
        {
            var transitions = workflow.GetTransitions(new Status("Other")).ToArray();

            Assert.Single(transitions);

            var (status1, step1, transition1) = transitions[0];

            Assert.Equal(Status.Draft, status1);
            Assert.Null(transition1?.Expression);
            Assert.Null(transition1?.Roles);
            Assert.Same(workflow.Steps[status1], step1);
        }
    }
}
