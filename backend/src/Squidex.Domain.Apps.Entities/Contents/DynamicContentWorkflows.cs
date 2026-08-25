// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class DynamicContentWorkflows(IScriptEngine scriptEngine, ILocalCache localCache) : IContentWorkflows
{
    public ValueTask<IContentWorkflow> GetWorkflowAsync(App app, Schema schema,
        CancellationToken ct = default)
    {
        // The definition is only cached for the current request, because the workflow must never be
        // stale. It is resolved several times per request, especially once per command guard.
        var cacheKey = (nameof(DynamicContentWorkflows), app.Id, app.Version, schema.Id, schema.Version);

        if (!localCache.TryGetValue(cacheKey, out var cached) || cached is not WorkflowDefinition definition)
        {
            definition = CreateDefinition(app, schema);

            localCache.Add(cacheKey, definition);
        }

        return new ValueTask<IContentWorkflow>(new DynamicContentWorkflow(definition, scriptEngine));
    }

    private static WorkflowDefinition CreateDefinition(App app, Schema schema)
    {
        var workflow =
            app.Workflows.Values.FirstOrDefault(x => x.SchemaIds.Contains(schema.Id)) ??
            app.Workflows.Values.FirstOrDefault(x => x.SchemaIds.Count == 0) ??
            Workflow.Default;

        return new WorkflowDefinition(workflow, schema.Properties.ValidateOnPublish);
    }
}
