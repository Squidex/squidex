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

namespace Squidex.Extensions.Actions.CreateContent;

[Obsolete("Has been replaced by flows.")]
public sealed record CreateContentAction : RuleAction
{
    [LocalizedRequired]
    public string Data { get; set; }

    [LocalizedRequired]
    public string Schema { get; set; }

    public string? Client { get; set; }

    public bool Publish { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new CreateContentFlowStep());
    }
}
