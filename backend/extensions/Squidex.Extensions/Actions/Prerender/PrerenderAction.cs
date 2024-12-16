// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Prerender;

public sealed record PrerenderAction : RuleAction<PrerenderStep>
{
    public string Token { get; set; }

    public string Url { get; set; }
}
