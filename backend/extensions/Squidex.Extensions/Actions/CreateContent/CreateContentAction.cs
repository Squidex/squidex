// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.CreateContent;

public sealed record CreateContentAction : RuleAction<CreateContentStep>
{
    public string Data { get; set; }

    public string Schema { get; set; }

    public string Client { get; set; }

    public bool Publish { get; set; }
}
