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

namespace Squidex.Extensions.Actions.Email;

[Obsolete("Use Flows")]
public sealed record EmailAction : RuleAction
{
    [LocalizedRequired]
    public string ServerHost { get; set; }

    [LocalizedRequired]
    public int ServerPort { get; set; }

    [LocalizedRequired]
    public string MessageFrom { get; set; }

    [LocalizedRequired]
    public string MessageTo { get; set; }

    [LocalizedRequired]
    public string MessageSubject { get; set; }

    [LocalizedRequired]
    public string MessageBody { get; set; }

    public string ServerUsername { get; set; }

    public string ServerPassword { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new EmailFlowStep());
    }
}
