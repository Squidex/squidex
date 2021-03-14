// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Test
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
                var scriptEngine = context.Services.GetRequiredService<IScriptEngine>();

                var vars = new ScriptVars
                {
                    Operation = "Create",
                    Data = data,
                    Status = status,
                    StatusOld = default
                }.Enrich(context);

                data = await scriptEngine.TransformAsync(vars, script, Options);
            }

            return data;
        }

        public static async Task<ContentData> ExecuteUpdateScriptAsync(this OperationContext context, ContentData data)
        {
            var script = context.SchemaDef.Scripts.Update;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var scriptEngine = context.Services.GetRequiredService<IScriptEngine>();

                var vars = new ScriptVars
                {
                    Operation = "Update",
                    Data = data,
                    DataOld = context.Content.Data,
                    Status = context.Content.EditingStatus,
                    StatusOld = default
                }.Enrich(context);

                data = await scriptEngine.TransformAsync(vars, script, Options);
            }

            return data;
        }

        public static async Task<ContentData> ExecuteChangeScriptAsync(this OperationContext context, Status status, StatusChange change)
        {
            var script = context.SchemaDef.Scripts.Change;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var data = context.Content.Data.Clone();

                var scriptEngine = context.Services.GetRequiredService<IScriptEngine>();

                var vars = new ScriptVars
                {
                    Operation = change.ToString(),
                    Data = data,
                    Status = status,
                    StatusOld = context.Content.EditingStatus
                }.Enrich(context);

                return await scriptEngine.TransformAsync(vars, script, Options);
            }

            return context.Content.Data;
        }

        public static async Task ExecuteDeleteScriptAsync(this OperationContext context)
        {
            var script = context.SchemaDef.Scripts.Delete;

            if (!string.IsNullOrWhiteSpace(script))
            {
                var scriptEngine = context.Services.GetRequiredService<IScriptEngine>();

                var vars = new ScriptVars
                {
                    Operation = "Delete",
                    Data = context.Content.Data,
                    Status = context.Content.EditingStatus,
                    StatusOld = default
                }.Enrich(context);

                await scriptEngine.ExecuteAsync(vars, script, Options);
            }
        }

        private static ScriptVars Enrich(this ScriptVars vars, OperationContext context)
        {
            vars.ContentId = context.ContentId;
            vars.AppId = context.App.Id;
            vars.AppName = context.App.Name;
            vars.User = context.User;

            return vars;
        }
    }
}
