// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ScriptContent : IContentEnricherStep
    {
        private readonly IScriptEngine scriptEngine;

        private static class ScriptKeys
        {
            public const string AppId = "appId";
            public const string AppName = "appName";
            public const string ContentId = "contentId";
            public const string Data = "data";
            public const string Operation = "operation";
            public const string User = "user";
        }

        public ScriptContent(IScriptEngine scriptEngine)
        {
            this.scriptEngine = scriptEngine;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            if (ShouldEnrich(context))
            {
                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var (schema, _) = await schemas(group.Key);

                    var script = schema.SchemaDef.Scripts.Query;

                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        await Task.WhenAll(group.Select(x => TransformAsync(context, script, x, ct)));
                    }
                }
            }
        }

        private async Task TransformAsync(Context context, string script, ContentEntity content,
            CancellationToken ct)
        {
            var vars = new ScriptVars
            {
                [ScriptKeys.AppId] = context.App.Id,
                [ScriptKeys.AppName] = context.App.Name,
                [ScriptKeys.ContentId] = content.Id,
                [ScriptKeys.Data] = content.Data,
                [ScriptKeys.User] = context.User
            };

            var options = new ScriptOptions
            {
                AsContext = true
            };

            content.Data = await scriptEngine.TransformAsync(vars, script, options, ct);
        }

        private static bool ShouldEnrich(Context context)
        {
            return !context.IsFrontendClient;
        }
    }
}
