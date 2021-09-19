// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public interface IRuleActionHandler
    {
        Type ActionType { get; }

        Type DataType { get; }

        Task<(string Description, object Data)> CreateJobAsync(EnrichedEvent @event, RuleAction action);

        Task<Result> ExecuteJobAsync(object data,
            CancellationToken ct = default);
    }
}
