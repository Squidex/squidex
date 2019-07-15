// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppWorkflowTests
    {
        private readonly Guid workflowId = Guid.NewGuid();
        private readonly Workflows workflows;

        public GuardAppWorkflowTests()
        {
            workflows = Workflows.Empty.Add(workflowId, "name");
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_name_is_not_defined()
        {
            var command = new AddWorkflow();

            ValidationAssert.Throws(() => GuardAppWorkflows.CanAdd(command),
                new ValidationError("Name is required.", "Name"));
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_command_is_valid()
        {
            var command = new AddWorkflow { Name = "my-workflow" };

            GuardAppWorkflows.CanAdd(command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_not_found()
        {
            var command = new UpdateWorkflow
            {
                Workflow = Workflow.Empty,
                WorkflowId = Guid.NewGuid()
            };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppWorkflows.CanUpdate(workflows, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_is_not_defined()
        {
            var command = new UpdateWorkflow { WorkflowId = workflowId };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Workflow is required.", "Workflow"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_has_no_initial_step()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    default,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = new WorkflowStep()
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Initial step is required.", "Workflow.Initial"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_initial_step_is_published()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    Status.Published,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = new WorkflowStep()
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Initial step cannot be published step.", "Workflow.Initial"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_does_not_have_published_state()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    Status.Draft,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Draft] = new WorkflowStep()
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Workflow must have a published step.", "Workflow.Steps"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_step_is_not_defined()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    Status.Draft,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = null,
                        [Status.Draft] = new WorkflowStep()
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Step is required.", "Workflow.Steps.Published"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_transition_is_invalid()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    Status.Draft,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] =
                            new WorkflowStep(
                                new Dictionary<Status, WorkflowTransition>
                                {
                                    [Status.Archived] = new WorkflowTransition()
                                }),
                        [Status.Draft] = new WorkflowStep()
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Transition has an invalid target.", "Workflow.Steps.Published.Transitions.Archived"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_workflow_transition_is_not_defined()
        {
            var command = new UpdateWorkflow
            {
                Workflow = new Workflow(
                    Status.Draft,
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Draft] =
                            new WorkflowStep(),
                        [Status.Published] =
                            new WorkflowStep(
                                new Dictionary<Status, WorkflowTransition>
                                {
                                    [Status.Draft] = null
                                })
                    }),
                WorkflowId = workflowId
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanUpdate(workflows, command),
                new ValidationError("Transition is required.", "Workflow.Steps.Published.Transitions.Draft"));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_workflow_is_valid()
        {
            var command = new UpdateWorkflow { Workflow = Workflow.Default, WorkflowId = workflowId };

            GuardAppWorkflows.CanUpdate(workflows, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_workflow_not_found()
        {
            var command = new DeleteWorkflow { WorkflowId = Guid.NewGuid() };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppWorkflows.CanDelete(workflows, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_workflow_is_found()
        {
            var command = new DeleteWorkflow { WorkflowId = workflowId };

            GuardAppWorkflows.CanDelete(workflows, command);
        }
    }
}
