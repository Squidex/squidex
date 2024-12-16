// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Discourse;

public sealed record DiscourseAction : RuleAction<DiscourseStep>
{
    public Uri Url { get; set; }

    public string ApiKey { get; set; }

    public string ApiUsername { get; set; }

    public string Text { get; set; }

    public string? Title { get; set; }

    public int? Topic { get; set; }

    public int? Category { get; set; }
}
