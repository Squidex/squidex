// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Fastly;

public sealed record FastlyAction : RuleAction<FastlyStep>
{
    public string ApiKey { get; set; }

    public string ServiceId { get; set; }
}
