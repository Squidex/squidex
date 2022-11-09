// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Contents;

public sealed record WorkflowStep(ReadonlyDictionary<Status, WorkflowTransition>? Transitions = null, string? Color = null, NoUpdate? NoUpdate = null, bool Validate = false)
{
    public ReadonlyDictionary<Status, WorkflowTransition> Transitions { get; } = Transitions ?? ReadonlyDictionary.Empty<Status, WorkflowTransition>();
}
