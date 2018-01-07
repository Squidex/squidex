// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleActionHandler
    {
        Type ActionType { get; }

        (string Description, RuleJobData Data) CreateJob(Envelope<AppEvent> @event, string eventName, RuleAction action);

        Task<(string Dump, Exception Exception)> ExecuteJobAsync(RuleJobData data);
    }
}
