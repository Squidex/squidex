// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class WorkflowExtensions
    {
        public static Task<Status> GetInitialStatusAsync(this ContentOperation operation)
        {
            var workflow = GetWorkflow(operation);

            return workflow.GetInitialStatusAsync(operation.Schema);
        }

        public static async Task CheckTransitionAsync(this ContentOperation operation, Status status)
        {
            if (operation.SchemaDef.Type != SchemaType.Singleton)
            {
                var workflow = GetWorkflow(operation);

                var oldStatus = operation.Snapshot.EditingStatus();

                if (!await workflow.CanMoveToAsync(operation.Snapshot, oldStatus, status, operation.User))
                {
                    var values = new { oldStatus, newStatus = status };

                    operation.AddError(T.Get("contents.statusTransitionNotAllowed", values), nameof(status));
                    operation.ThrowOnErrors();
                }
            }
        }

        public static async Task CheckStatusAsync(this ContentOperation operation, Status status)
        {
            if (operation.SchemaDef.Type != SchemaType.Singleton)
            {
                var workflow = GetWorkflow(operation);

                var statusInfo = await workflow.GetInfoAsync(operation.Snapshot, status);

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
                var workflow = GetWorkflow(operation);

                var status = operation.Snapshot.EditingStatus();

                if (!await workflow.CanUpdateAsync(operation.Snapshot, status, operation.User))
                {
                    throw new DomainException(T.Get("contents.workflowErrorUpdate", new { status }));
                }
            }
        }

        private static IContentWorkflow GetWorkflow(ContentOperation operation)
        {
            return operation.Resolve<IContentWorkflow>();
        }
    }
}
