// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class DynamicContentWorkflow : IContentWorkflow
    {
        private readonly IScriptEngine scriptEngine;
        private readonly IAppProvider appProvider;

        public DynamicContentWorkflow(IScriptEngine scriptEngine, IAppProvider appProvider)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.scriptEngine = scriptEngine;

            this.appProvider = appProvider;
        }

        public async Task<StatusInfo[]> GetAllAsync(ISchemaEntity schema)
        {
            var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

            return workflow.Steps.Select(x => new StatusInfo(x.Key, GetColor(x.Value))).ToArray();
        }

        public async Task<bool> CanMoveToAsync(IContentEntity content, Status next, ClaimsPrincipal user)
        {
            var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

            return workflow.TryGetTransition(content.Status, next, out var transition) && CanUse(transition, content.DataDraft, user);
        }

        public async Task<bool> CanPublishOnCreateAsync(ISchemaEntity schema, NamedContentData data, ClaimsPrincipal user)
        {
            var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

            return workflow.TryGetTransition(workflow.Initial, Status.Published, out var transition) && CanUse(transition, data, user);
        }

        public async Task<bool> CanUpdateAsync(IContentEntity content)
        {
            var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

            if (workflow.TryGetStep(content.Status, out var step))
            {
                return !step.NoUpdate;
            }

            return true;
        }

        public async Task<StatusInfo> GetInfoAsync(IContentEntity content)
        {
            var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

            if (workflow.TryGetStep(content.Status, out var step))
            {
                return new StatusInfo(content.Status, GetColor(step));
            }

            return new StatusInfo(content.Status, StatusColors.Draft);
        }

        public async Task<StatusInfo> GetInitialStatusAsync(ISchemaEntity schema)
        {
            var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

            var (status, step) = workflow.GetInitialStep();

            return new StatusInfo(status, GetColor(step));
        }

        public async Task<StatusInfo[]> GetNextsAsync(IContentEntity content, ClaimsPrincipal user)
        {
            var result = new List<StatusInfo>();

            var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

            foreach (var (to, step, transition) in workflow.GetTransitions(content.Status))
            {
                if (CanUse(transition, content.DataDraft, user))
                {
                    result.Add(new StatusInfo(to, GetColor(step)));
                }
            }

            return result.ToArray();
        }

        private bool CanUse(WorkflowTransition transition, NamedContentData data, ClaimsPrincipal user)
        {
            if (!string.IsNullOrWhiteSpace(transition.Role))
            {
                if (!user.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == transition.Role))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(transition.Expression))
            {
                return scriptEngine.Evaluate("data", data, transition.Expression);
            }

            return true;
        }

        private async Task<Workflow> GetWorkflowAsync(Guid appId, Guid schemaId)
        {
            Workflow result = null;

            var app = await appProvider.GetAppAsync(appId);

            if (app != null)
            {
                result = app.Workflows.Values.FirstOrDefault(x => x.SchemaIds.Contains(schemaId));

                if (result == null)
                {
                    result = app.Workflows.Values.FirstOrDefault(x => x.SchemaIds.Count == 0);
                }
            }

            if (result == null)
            {
                result = Workflow.Default;
            }

            return result;
        }

        private static string GetColor(WorkflowStep step)
        {
            return step.Color ?? StatusColors.Draft;
        }
    }
}
