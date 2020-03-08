// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class ScriptContent : IContentEnricherStep
    {
        private readonly IScriptEngine scriptEngine;

        public ScriptContent(IScriptEngine scriptEngine)
        {
            Guard.NotNull(scriptEngine, nameof(scriptEngine));

            this.scriptEngine = scriptEngine;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            if (ShouldEnrich(context))
            {
                foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
                {
                    var schema = await schemas(group.Key);

                    var script = schema.SchemaDef.Scripts.Query;

                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        var results = new List<IEnrichedContentEntity>();

                        await Task.WhenAll(group.Select(x => TransformAsync(context, script, x)));
                    }
                }
            }
        }

        private async Task TransformAsync(Context context, string script, ContentEntity content)
        {
            var scriptContext = new ScriptContext { User = context.User };

            scriptContext.Data = content.Data;
            scriptContext.ContentId = content.Id;

            content.Data = await scriptEngine.TransformAsync(scriptContext, script);
        }

        private static bool ShouldEnrich(Context context)
        {
            return !context.IsFrontendClient;
        }
    }
}
