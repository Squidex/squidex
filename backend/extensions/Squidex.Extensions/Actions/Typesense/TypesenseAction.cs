// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.Typesense;

public sealed record TypesenseAction : RuleAction<TypesenseStep>
{
    public Uri Host { get; set; }

    public string IndexName { get; set; }

    public string ApiKey { get; set; }

    public string? Document { get; set; }

    public string? Delete { get; set; }
}
