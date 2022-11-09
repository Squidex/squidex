// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents;

public abstract record WorkflowCondition
{
    public string? Expression { get; init; }

    public ReadonlyList<string>? Roles { get; init; }
}
