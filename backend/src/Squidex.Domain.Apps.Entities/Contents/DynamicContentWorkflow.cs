// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class DynamicContentWorkflow(WorkflowDefinition definition, IScriptEngine scriptEngine) : IContentWorkflow
{
    // The same expression is evaluated for every content of a batch, therefore the compiled script is
    // kept around. A script that cannot be compiled is stored as null to not retry it over and over.
    private readonly Dictionary<string, IScript?> scripts = [];

    // The next statuses of a step without any condition are the same for every content and user.
    private readonly Dictionary<Status, StatusInfo[]> nextStatuses = [];

    public StatusInfo[] GetAll()
    {
        return definition.AllStatuses;
    }

    public Status GetInitialStatus()
    {
        return definition.Workflow.Initial;
    }

    public StatusInfo? GetInfo(Status status)
    {
        return definition.StatusInfos.GetValueOrDefault(status);
    }

    public bool ShouldValidate(Status status)
    {
        if (definition.Workflow.TryGetStep(status, out var step) && step.Validate)
        {
            return true;
        }

        return status == Status.Published && definition.ValidateOnPublish;
    }

    public bool CanPublishInitial(ClaimsPrincipal? user)
    {
        var workflow = definition.Workflow;

        return workflow.TryGetTransition(workflow.Initial, Status.Published, out var transition) && IsTrue(transition, null, user);
    }

    public bool CanMoveTo(Content content, Status status, Status next, ClaimsPrincipal? user)
    {
        return definition.Workflow.TryGetTransition(status, next, out var transition) && IsTrue(transition, content.Data, user);
    }

    public bool CanUpdate(Content content, Status status, ClaimsPrincipal? user)
    {
        if (definition.Workflow.TryGetStep(status, out var step))
        {
            return step.NoUpdate == null || !IsTrue(step.NoUpdate, content.Data, user);
        }

        return true;
    }

    public StatusInfo[] GetNext(Content content, Status status, ClaimsPrincipal? user)
    {
        if (nextStatuses.TryGetValue(status, out var cached))
        {
            return cached;
        }

        List<StatusInfo>? result = null;

        var isStatic = true;
        foreach (var (to, _, transition) in definition.Workflow.GetTransitions(status))
        {
            isStatic = isStatic && IsUnconditional(transition);

            if (IsTrue(transition, content.Data, user))
            {
                result ??= [];
                result.Add(definition.StatusInfos[to]);
            }
        }

        var statuses = result?.ToArray() ?? [];
        if (isStatic)
        {
            nextStatuses[status] = statuses;
        }

        return statuses;
    }

    public void Dispose()
    {
        foreach (var script in scripts.Values)
        {
            script?.Dispose();
        }

        scripts.Clear();
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
            var script = GetScript(condition.Expression);
            if (script == null)
            {
                return false;
            }

            var vars = new DataScriptVars
            {
                Data = data,
            };

            return script.Evaluate(vars);
        }

        return true;
    }

    private IScript? GetScript(string expression)
    {
        if (scripts.TryGetValue(expression, out var script))
        {
            return script;
        }

        try
        {
            script = scriptEngine.CreateScript(expression);
        }
        catch
        {
            script = null;
        }

        scripts[expression] = script;
        return script;
    }

    private static bool IsUnconditional(WorkflowCondition condition)
    {
        return condition.Roles == null && string.IsNullOrWhiteSpace(condition.Expression);
    }
}
