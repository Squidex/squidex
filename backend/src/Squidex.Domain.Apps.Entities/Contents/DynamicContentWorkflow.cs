// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class DynamicContentWorkflow : IContentWorkflow
{
    private readonly IScriptEngine scriptEngine;
    private readonly IAppProvider appProvider;

    public DynamicContentWorkflow(IScriptEngine scriptEngine, IAppProvider appProvider)
    {
        this.scriptEngine = scriptEngine;

        this.appProvider = appProvider;
    }

    public async ValueTask<StatusInfo[]> GetAllAsync(Schema schema)
    {
        var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

        return workflow.Steps.Select(x => new StatusInfo(x.Key, GetColor(x.Value))).ToArray();
    }

    public async ValueTask<bool> CanPublishInitialAsync(Schema schema, ClaimsPrincipal? user)
    {
        var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

        return workflow.TryGetTransition(workflow.Initial, Status.Published, out var transition) && IsTrue(transition, null, user);
    }

    public async ValueTask<bool> CanMoveToAsync(Content content, Status status, Status next, ClaimsPrincipal? user)
    {
        var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

        return workflow.TryGetTransition(status, next, out var transition) && IsTrue(transition, content.Data, user);
    }

    public async ValueTask<bool> CanUpdateAsync(Content content, Status status, ClaimsPrincipal? user)
    {
        var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

        if (workflow.TryGetStep(status, out var step))
        {
            return step.NoUpdate == null || !IsTrue(step.NoUpdate, content.Data, user);
        }

        return true;
    }

    public async ValueTask<bool> ShouldValidateAsync(Schema schema, Status status)
    {
        var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

        if (workflow.TryGetStep(status, out var step) && step.Validate)
        {
            return true;
        }

        return status == Status.Published && schema.Properties.ValidateOnPublish;
    }

    public async ValueTask<StatusInfo?> GetInfoAsync(Content content, Status status)
    {
        var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

        if (workflow.TryGetStep(status, out var step))
        {
            return new StatusInfo(status, GetColor(step));
        }

        return null;
    }

    public async ValueTask<Status> GetInitialStatusAsync(Schema schema)
    {
        var workflow = await GetWorkflowAsync(schema.AppId.Id, schema.Id);

        var (status, _) = workflow.GetInitialStep();

        return status;
    }

    public async ValueTask<StatusInfo[]> GetNextAsync(Content content, Status status, ClaimsPrincipal? user)
    {
        var result = new List<StatusInfo>();

        var workflow = await GetWorkflowAsync(content.AppId.Id, content.SchemaId.Id);

        foreach (var (to, step, transition) in workflow.GetTransitions(status))
        {
            if (IsTrue(transition, content.Data, user))
            {
                result.Add(new StatusInfo(to, GetColor(step)));
            }
        }

        return result.ToArray();
    }

    private bool IsTrue(WorkflowCondition condition, ContentData? data, ClaimsPrincipal? user)
    {
        if (condition?.Roles != null && user != null)
        {
            if (!user.Claims.Any(x => x.Type == ClaimTypes.Role && condition.Roles.Contains(x.Value)))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(condition?.Expression) && data != null)
        {
            var vars = new DataScriptVars
            {
                Data = data
            };

            return scriptEngine.Evaluate(vars, condition.Expression);
        }

        return true;
    }

    private async ValueTask<Workflow> GetWorkflowAsync(DomainId appId, DomainId schemaId)
    {
        Workflow? result = null;

        var app = await appProvider.GetAppAsync(appId, false);

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
