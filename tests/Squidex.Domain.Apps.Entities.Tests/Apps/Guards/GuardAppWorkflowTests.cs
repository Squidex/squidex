// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_is_not_defined()
        {
            var command = new ConfigureWorkflow();

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Workflow is required.", "Workflow"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_has_no_initial_step()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = new WorkflowStep()
                    },
                    default)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Initial step is required.", "Workflow.Initial"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_initial_step_is_published()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = new WorkflowStep()
                    },
                    Status.Published)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Initial step cannot be published step.", "Workflow.Initial"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_does_not_have_published_state()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Draft] = new WorkflowStep()
                    },
                    Status.Draft)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Workflow must have a published step.", "Workflow.Steps"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_step_is_not_defined()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] = null,
                        [Status.Draft] = new WorkflowStep()
                    },
                    Status.Draft)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Step is required.", "Workflow.Steps.Published"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_transition_is_invalid()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
                    new Dictionary<Status, WorkflowStep>
                    {
                        [Status.Published] =
                            new WorkflowStep(
                                new Dictionary<Status, WorkflowTransition>
                                {
                                    [Status.Archived] = new WorkflowTransition()
                                }),
                        [Status.Draft] = new WorkflowStep()
                    },
                    Status.Draft)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Transition has an invalid target.", "Workflow.Steps.Published.Transitions.Archived"));
        }

        [Fact]
        public void CanConfigure_should_throw_exception_if_workflow_transition_is_not_defined()
        {
            var command = new ConfigureWorkflow
            {
                Workflow = new Workflow(
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
                    },
                    Status.Draft)
            };

            ValidationAssert.Throws(() => GuardAppWorkflows.CanConfigure(command),
                new ValidationError("Transition is required.", "Workflow.Steps.Published.Transitions.Draft"));
        }

        [Fact]
        public void CanConfigure_should_not_throw_exception_if_workflow_is_valid()
        {
            var command = new ConfigureWorkflow { Workflow = Workflow.Default };

            GuardAppWorkflows.CanConfigure(command);
        }
    }
}
