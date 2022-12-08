// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public sealed class AddWorkflow : AppCommand
{
    public DomainId WorkflowId { get; set; }

    public string Name { get; set; }

    public AddWorkflow()
    {
        WorkflowId = DomainId.NewGuid();
    }
}
