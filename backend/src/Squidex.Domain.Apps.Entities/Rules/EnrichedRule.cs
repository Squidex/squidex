// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed record EnrichedRule : Rule
{
    public long NumSucceeded { get; init; }

    public long NumFailed { get; init; }
}
