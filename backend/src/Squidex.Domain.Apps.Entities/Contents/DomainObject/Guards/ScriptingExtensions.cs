// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class ScriptingExtensions
    {
        private static readonly ScriptOptions Options = new ScriptOptions
        {
            AsContext = true,
            CanDisallow = true,
            CanReject = true
        };

        private static class ScriptKeys
        {
            public const string AppId = "appId";
            public const string AppName = "appName";
            public const string Command = "command";
            public const string Content = "content";
            public const string ContentId = "contentId";
            public const string Data = "data";
            public const string DataOld = "dataOld";
            public const string OldData = "oldData";
            public const string OldStatus = "oldStatus";
            public const string Operation = "operation";
            public const string SchemaId = "achemaId";
            public const string SchemaName = "achemaName";
            public const string Status = "status";
            public const string StatusOld = "statusOld";
            public const string User = "user";
        }

        public static Task<ContentData> ExecuteCreateScriptAsync(this ContentOperation operation, ContentData data, Status status)
        {
            var script = operation.SchemaDef.Scripts.Create;

            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.FromResult(data);
            }

            var vars = Enrich(operation, new ScriptVars
            {
                [ScriptKeys.Data] = data,
                [ScriptKeys.DataOld] = null,
                [ScriptKeys.OldData] = null,
                [ScriptKeys.OldStatus] = default(Status),
                [ScriptKeys.Operation] = "Create",
                [ScriptKeys.Status] = status,
                [ScriptKeys.StatusOld] = default(Status)
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

            var vars = Enrich(operation, new ScriptVars
            {
                [ScriptKeys.Data] = data,
                [ScriptKeys.DataOld] = operation.Snapshot.Data,
                [ScriptKeys.OldData] = operation.Snapshot.Data,
                [ScriptKeys.OldStatus] = data,
                [ScriptKeys.Operation] = "Update",
                [ScriptKeys.Status] = operation.Snapshot.EditingStatus(),
                [ScriptKeys.StatusOld] = default(Status)
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

            // Clone the data so we do not change it by accident.
            var data = operation.Snapshot.Data.Clone();

            var vars = Enrich(operation, new ScriptVars
            {
                [ScriptKeys.Data] = data,
                [ScriptKeys.DataOld] = null,
                [ScriptKeys.OldData] = null,
                [ScriptKeys.OldStatus] = operation.Snapshot.EditingStatus(),
                [ScriptKeys.Operation] = change.ToString(),
                [ScriptKeys.Status] = status,
                [ScriptKeys.StatusOld] = operation.Snapshot.EditingStatus()
            });

            return TransformAsync(operation, script, vars);
        }

        public static Task ExecuteDeleteScriptAsync(this ContentOperation operation)
        {
            var script = operation.SchemaDef.Scripts.Delete;

            if (string.IsNullOrWhiteSpace(script))
            {
                return Task.CompletedTask;
            }

            var vars = Enrich(operation, new ScriptVars
            {
                [ScriptKeys.Data] = operation.Snapshot.Data,
                [ScriptKeys.DataOld] = null,
                [ScriptKeys.OldData] = null,
                [ScriptKeys.OldStatus] = operation.Snapshot.EditingStatus(),
                [ScriptKeys.Operation] = "Delete",
                [ScriptKeys.Status] = operation.Snapshot.EditingStatus(),
                [ScriptKeys.StatusOld] = default(Status)
            });

            return ExecuteAsync(operation, script, vars);
        }

        private static async Task<ContentData> TransformAsync(ContentOperation operation, string script, ScriptVars vars)
        {
            return await operation.Resolve<IScriptEngine>().TransformAsync(vars, script, Options);
        }

        private static async Task ExecuteAsync(ContentOperation operation, string script, ScriptVars vars)
        {
            await operation.Resolve<IScriptEngine>().ExecuteAsync(vars, script, Options);
        }

        private static ScriptVars Enrich(ContentOperation operation, ScriptVars vars)
        {
            vars[ScriptKeys.AppId] = operation.App.Id;
            vars[ScriptKeys.AppName] = operation.App.Name;
            vars[ScriptKeys.Command] = operation.Command;
            vars[ScriptKeys.Content] = operation.Snapshot;
            vars[ScriptKeys.ContentId] = operation.CommandId;
            vars[ScriptKeys.SchemaId] = operation.Schema.Id;
            vars[ScriptKeys.SchemaName] = operation.Schema.SchemaDef.Name;
            vars[ScriptKeys.User] = operation.User;

            return vars;
        }
    }
}
