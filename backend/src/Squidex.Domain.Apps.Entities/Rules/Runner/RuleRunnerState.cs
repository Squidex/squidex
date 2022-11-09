// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

[CollectionName("Rules_Runner")]
public sealed class RuleRunnerState
{
    public DomainId RuleId { get; set; }

    public DomainId RunId { get; set; }

    public string? Position { get; set; }

    public bool RunFromSnapshots { get; set; }
}
