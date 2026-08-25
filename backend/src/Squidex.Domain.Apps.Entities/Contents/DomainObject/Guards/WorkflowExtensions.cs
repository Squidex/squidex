// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

public static class WorkflowExtensions
{
    public static async ValueTask<Status> GetInitialStatusAsync(this ContentOperation operation)
    {
        using var workflow = await GetWorkflowAsync(operation);

        return workflow.GetInitialStatus();
    }

    public static async ValueTask<bool> ShouldValidateAsync(this ContentOperation operation, Status status)
    {
        using var workflow = await GetWorkflowAsync(operation);

        return workflow.ShouldValidate(status);
    }

    public static async Task CheckTransitionAsync(this ContentOperation operation, Status status)
    {
        if (operation.Schema.Type != SchemaType.Singleton)
        {
            using var workflow = await GetWorkflowAsync(operation);

            if (!workflow.CanMoveTo(operation.Snapshot.ToContent(), operation.Snapshot.EditingStatus, status, operation.User))
            {
                var values = new { oldStatus = operation.Snapshot.EditingStatus, newStatus = status };

                operation.AddError(T.Get("contents.statusTransitionNotAllowed", values), nameof(status));
                operation.ThrowOnErrors();
            }
        }
    }

    public static async Task CheckStatusAsync(this ContentOperation operation, Status status)
    {
        if (operation.Schema.Type != SchemaType.Singleton)
        {
            using var workflow = await GetWorkflowAsync(operation);

            var statusInfo = workflow.GetInfo(status);

            if (statusInfo == null)
            {
                operation.AddError(T.Get("contents.statusNotValid"), nameof(status));
                operation.ThrowOnErrors();
            }
        }
    }

    public static async Task CheckUpdateAsync(this ContentOperation operation)
    {
        if (operation.User != null)
        {
            using var workflow = await GetWorkflowAsync(operation);

            if (!workflow.CanUpdate(operation.Snapshot.ToContent(), operation.Snapshot.EditingStatus, operation.User))
            {
                throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status = operation.Snapshot.EditingStatus }));
            }
        }
    }

    private static ValueTask<IContentWorkflow> GetWorkflowAsync(ContentOperation operation)
    {
        return operation.Resolve<IContentWorkflows>().GetWorkflowAsync(operation.App, operation.Schema);
    }
}
