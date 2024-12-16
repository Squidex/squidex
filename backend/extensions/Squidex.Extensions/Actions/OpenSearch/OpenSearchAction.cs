// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Old;

namespace Squidex.Extensions.Actions.OpenSearch;

public sealed record OpenSearchAction : RuleAction<OpenSearchStep>
{
    public Uri Host { get; set; }

    public string IndexName { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Document { get; set; }

    public string? Delete { get; set; }
}
