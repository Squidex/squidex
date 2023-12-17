// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents.Json;

public sealed class WorkflowTransitionSurrogate : ISurrogate<WorkflowTransition>
{
    public string? Expression { get; set; }

    public string[]? Roles { get; set; }

    public string? Role
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                Roles = [value];
            }
        }
    }

    public void FromSource(WorkflowTransition source)
    {
        Roles = source.Roles?.ToArray();

        Expression = source.Expression;
    }

    public WorkflowTransition ToSource()
    {
        var roles = Roles;

        return WorkflowTransition.When(Expression, roles);
    }
}
