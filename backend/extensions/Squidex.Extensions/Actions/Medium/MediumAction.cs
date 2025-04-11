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

namespace Squidex.Extensions.Actions.Medium;

[Obsolete("Use Flows")]
public sealed record MediumAction : RuleAction
{
    [LocalizedRequired]
    public string AccessToken { get; set; }

    [LocalizedRequired]
    public string Title { get; set; }

    [LocalizedRequired]
    public string Content { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? Tags { get; set; }

    public string? PublicationId { get; set; }

    public bool IsHtml { get; set; }

    public override FlowStep ToFlowStep()
    {
        return SimpleMapper.Map(this, new MediumFlowStep());
    }
}
