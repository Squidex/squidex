// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardAppWorkflows
    {
        public static void CanAdd(AddWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot add workflow.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                {
                    e(Not.Defined("Name"), nameof(command.Name));
                }
            });
        }

        public static void CanUpdate(Workflows workflows, UpdateWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            GetWorkflowOrThrow(workflows, command.WorkflowId);

            Validate.It(() => "Cannot update workflow.", e =>
            {
                if (command.Workflow == null)
                {
                    e(Not.Defined("Workflow"), nameof(command.Workflow));
                    return;
                }

                var workflow = command.Workflow;

                if (!workflow.Steps.ContainsKey(workflow.Initial))
                {
                    e(Not.Defined("Initial step"), $"{nameof(command.Workflow)}.{nameof(workflow.Initial)}");
                }

                if (workflow.Initial == Status.Published)
                {
                    e("Initial step cannot be published step.", $"{nameof(command.Workflow)}.{nameof(workflow.Initial)}");
                }

                var stepsPrefix = $"{nameof(command.Workflow)}.{nameof(workflow.Steps)}";

                if (!workflow.Steps.ContainsKey(Status.Published))
                {
                    e("Workflow must have a published step.", stepsPrefix);
                }

                foreach (var step in workflow.Steps)
                {
                    var stepPrefix = $"{stepsPrefix}.{step.Key}";

                    if (step.Value == null)
                    {
                        e(Not.Defined("Step"), stepPrefix);
                    }
                    else
                    {
                        foreach (var transition in step.Value.Transitions)
                        {
                            var transitionPrefix = $"{stepPrefix}.{nameof(step.Value.Transitions)}.{transition.Key}";

                            if (!workflow.Steps.ContainsKey(transition.Key))
                            {
                                e("Transition has an invalid target.", transitionPrefix);
                            }

                            if (transition.Value == null)
                            {
                                e(Not.Defined("Transition"), transitionPrefix);
                            }
                        }
                    }
                }
            });
        }

        public static void CanDelete(Workflows workflows, DeleteWorkflow command)
        {
            Guard.NotNull(command, nameof(command));

            GetWorkflowOrThrow(workflows, command.WorkflowId);
        }

        private static Workflow GetWorkflowOrThrow(Workflows workflows, Guid id)
        {
            if (!workflows.TryGetValue(id, out var workflow))
            {
                throw new DomainObjectNotFoundException(id.ToString(), "Workflows", typeof(IAppEntity));
            }

            return workflow;
        }
    }
}
