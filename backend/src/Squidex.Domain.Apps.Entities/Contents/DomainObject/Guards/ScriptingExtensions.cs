// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

public static class ScriptingExtensions
{
    private static readonly ScriptOptions Options = new ScriptOptions
    {
        AsContext = true,
        CanDisallow = true,
        CanReject = true
    };

    public static Task<ContentData> ExecuteCreateScriptAsync(this ContentOperation operation, ContentData data, Status status)
    {
        var script = operation.SchemaDef.Scripts.Create;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.FromResult(data);
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = Enrich(operation, new ContentScriptVars
        {
            Data = data,
            DataOld = null,
            OldData = null,
            OldStatus = default,
            Operation = "Create",
            Status = status,
            StatusOld = default
        });

        return TransformAsync(operation, script, vars);
    }

    public static Task<ContentData> ExecuteUpdateScriptAsync(this ContentOperation operation, ContentData data)
    {
        var script = operation.SchemaDef.Scripts.Update;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.FromResult(data);
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = Enrich(operation, new ContentScriptVars
        {
            Data = data,
            DataOld = operation.Snapshot.Data,
            OldData = operation.Snapshot.Data,
            OldStatus = operation.Snapshot.Status,
            Operation = "Update",
            Status = operation.Snapshot.EditingStatus(),
            StatusOld = default
        });

        return TransformAsync(operation, script, vars);
    }

    public static Task<ContentData> ExecuteChangeScriptAsync(this ContentOperation operation, Status status, StatusChange change)
    {
        var script = operation.SchemaDef.Scripts.Change;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.FromResult(operation.Snapshot.Data);
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = Enrich(operation, new ContentScriptVars
        {
            Data = operation.Snapshot.Data.Clone(),
            DataOld = null,
            OldData = null,
            OldStatus = operation.Snapshot.EditingStatus(),
            Operation = change.ToString(),
            Status = status,
            StatusOld = operation.Snapshot.EditingStatus(),
            Validate = Validate(operation, status)
        });

        return TransformAsync(operation, script, vars);
    }

    public static Task ExecuteDeleteScriptAsync(this ContentOperation operation, bool permanent)
    {
        var script = operation.SchemaDef.Scripts.Delete;

        if (string.IsNullOrWhiteSpace(script))
        {
            return Task.CompletedTask;
        }

        // Script vars are just wrappers over dictionaries for better performance.
        var vars = Enrich(operation, new ContentScriptVars
        {
            Data = operation.Snapshot.Data,
            DataOld = null,
            OldData = null,
            OldStatus = operation.Snapshot.EditingStatus(),
            Operation = "Delete",
            Permanent = permanent,
            Status = operation.Snapshot.EditingStatus(),
            StatusOld = default
        });

        return ExecuteAsync(operation, script, vars);
    }

    private static async Task<ContentData> TransformAsync(ContentOperation operation, string script, ContentScriptVars vars)
    {
        return await operation.Resolve<IScriptEngine>().TransformAsync(vars, script, Options);
    }

    private static async Task ExecuteAsync(ContentOperation operation, string script, ContentScriptVars vars)
    {
        await operation.Resolve<IScriptEngine>().ExecuteAsync(vars, script, Options);
    }

    private static Action Validate(ContentOperation operation, Status status)
    {
        return () =>
        {
            try
            {
                var snapshot = operation.Snapshot;

                operation.ValidateContentAndInputAsync(snapshot.Data, false, snapshot.IsPublished() || status == Status.Published).Wait();
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                throw ex.Flatten().InnerException!;
            }
        };
    }

    private static ContentScriptVars Enrich(ContentOperation operation, ContentScriptVars vars)
    {
        vars.AppId = operation.App.Id;
        vars.AppName = operation.App.Name;
        vars.ContentId = operation.CommandId;
        vars.SchemaId = operation.Schema.Id;
        vars.SchemaName = operation.Schema.SchemaDef.Name;
        vars.User = operation.User;

        return vars;
    }
}
