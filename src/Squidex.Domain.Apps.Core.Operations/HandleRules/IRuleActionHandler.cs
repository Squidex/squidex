// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleActionHandler
    {
        Type ActionType { get; }

        Task<(string Description, JObject Data)> CreateJobAsync(Envelope<AppEvent> @event, string eventName, RuleAction action);

        Task<(string Dump, Exception Exception)> ExecuteJobAsync(JObject data);
    }
}
