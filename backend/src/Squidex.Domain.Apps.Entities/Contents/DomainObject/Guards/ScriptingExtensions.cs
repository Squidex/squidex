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

        public static async Task<ContentData> ExecuteCreateScriptAsync(this OperationContext context, ContentData data, Status status)
        {
            var script = context.SchemaDef.Scripts.Create;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var vars = Enrich(context, new ScriptVars
                {
                    Operation = "Create",
                    Data = data,
                    DataOld = default,
                    Status = status,
                    StatusOld = default
                });

                data = await GetScriptEngine(context).TransformAsync(vars, script, Options);
            }

            return data;
        }

        public static async Task<ContentData> ExecuteUpdateScriptAsync(this OperationContext context, ContentData data)
        {
            var script = context.SchemaDef.Scripts.Update;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var vars = Enrich(context, new ScriptVars
                {
                    Operation = "Update",
                    Data = data,
                    DataOld = context.Content.Data,
                    Status = context.Content.EditingStatus(),
                    StatusOld = default
                });

                data = await GetScriptEngine(context).TransformAsync(vars, script, Options);
            }

            return data;
        }

        public static async Task<ContentData> ExecuteChangeScriptAsync(this OperationContext context, Status status, StatusChange change)
        {
            var script = context.SchemaDef.Scripts.Change;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var data = context.Content.Data.Clone();

                var vars = Enrich(context, new ScriptVars
                {
                    Operation = change.ToString(),
                    Data = data,
                    DataOld = default,
                    Status = status,
                    StatusOld = context.Content.EditingStatus()
                });

                return await GetScriptEngine(context).TransformAsync(vars, script, Options);
            }

            return context.Content.Data;
        }

        public static async Task ExecuteDeleteScriptAsync(this OperationContext context)
        {
            var script = context.SchemaDef.Scripts.Delete;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var vars = Enrich(context, new ScriptVars
                {
                    Operation = "Delete",
                    Data = context.Content.Data,
                    DataOld = default,
                    Status = context.Content.EditingStatus(),
                    StatusOld = default
                });

                await GetScriptEngine(context).ExecuteAsync(vars, script, Options);
            }
        }

        private static IScriptEngine GetScriptEngine(OperationContext context)
        {
            return context.Resolve<IScriptEngine>();
        }

        private static ScriptVars Enrich(OperationContext context, ScriptVars vars)
        {
            vars.ContentId = context.ContentId;
            vars.AppId = context.App.Id;
            vars.AppName = context.App.Name;
            vars.SchemaId = context.Schema.Id;
            vars.SchemaName = context.Schema.SchemaDef.Name;
            vars.User = context.User;

            return vars;
        }
    }
}
