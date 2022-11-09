// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class CreateClientDto
{
    /// <summary>
    /// The ID of the client.
    /// </summary>
    [LocalizedRequired]
    [LocalizedRegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
    public string Id { get; set; }

    public AttachClient ToCommand()
    {
        return SimpleMapper.Map(this, new AttachClient());
    }
}
