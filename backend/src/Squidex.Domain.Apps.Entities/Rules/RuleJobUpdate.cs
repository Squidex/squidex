// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleJobUpdate
{
    public string? ExecutionDump { get; set; }

    public RuleResult ExecutionResult { get; set; }

    public RuleJobResult JobResult { get; set; }

    public TimeSpan Elapsed { get; set; }

    public Instant Finished { get; set; }

    public Instant? JobNext { get; set; }
}
