// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleService
    {
        bool CanCreateSnapshotEvents(Rule rule);

        IAsyncEnumerable<(RuleJob? Job, Exception? Exception)> CreateSnapshotJobsAsync(Rule rule, DomainId ruleId, DomainId appId);

        Task<List<(RuleJob Job, Exception? Exception)>> CreateJobsAsync(Rule rule, DomainId ruleId, Envelope<IEvent> @event, bool ignoreStale = true);

        Task<(Result Result, TimeSpan Elapsed)> InvokeAsync(string actionName, string job);
    }
}