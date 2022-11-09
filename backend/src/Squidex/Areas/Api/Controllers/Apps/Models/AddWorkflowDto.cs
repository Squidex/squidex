// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class AddWorkflowDto
{
    /// <summary>
    /// The name of the workflow.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    public AddWorkflow ToCommand()
    {
        return new AddWorkflow { Name = Name };
    }
}
