// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Extensions.Actions.Script
{
    public sealed class ScriptActionHandler : RuleActionHandler<ScriptAction, ScriptJob>
    {
        private readonly IScriptEngine scriptEngine;

        public ScriptActionHandler(RuleEventFormatter formatter, IScriptEngine scriptEngine)
            : base(formatter)
        {
            this.scriptEngine = scriptEngine;
        }

        protected override Task<(string Description, ScriptJob Data)> CreateJobAsync(EnrichedEvent @event, ScriptAction action)
        {
            var job = new ScriptJob { Script = action.Script, Event = @event };

            return Task.FromResult(($"Run a script", job));
        }

        protected override async Task<Result> ExecuteJobAsync(ScriptJob job,
            CancellationToken ct = default)
        {
            var vars = new ScriptVars
            {
                ["event"] = job.Event
            };

            var result = await scriptEngine.ExecuteAsync(vars, job.Script, ct: ct);

            return Result.Success(result?.ToString());
        }
    }

    public sealed class ScriptJob
    {
        public EnrichedEvent Event { get; set; }

        public string Script { get; set; }
    }
}
