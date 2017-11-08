// ==========================================================================
//  IRuleActionHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleActionHandler
    {
        Type ActionType { get; }

        (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, RuleAction action);

        Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData data);
    }
}
