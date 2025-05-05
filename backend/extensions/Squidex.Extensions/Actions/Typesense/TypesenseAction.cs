// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Extensions.Actions.Typesense;

[Obsolete("Has been replaced by flows.")]
public sealed record TypesenseAction : RuleAction
{
    [AbsoluteUrl]
    [LocalizedRequired]
    public Uri Host { get; set; }

    [LocalizedRequired]
    public string IndexName { get; set; }

    public string ApiKey { get; set; }

    public string? Document { get; set; }

    public string? Delete { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new TypesenseFlowStep());
    }
}
