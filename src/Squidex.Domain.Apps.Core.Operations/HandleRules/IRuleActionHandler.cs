// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleActionHandler
    {
        Type ActionType { get; }

        Type DataType { get; }

        Task<(string Description, object Data)> CreateJobAsync(EnrichedEvent @event, RuleAction action);

        Task<(string Dump, Exception Exception)> ExecuteJobAsync(object data);
    }
}
