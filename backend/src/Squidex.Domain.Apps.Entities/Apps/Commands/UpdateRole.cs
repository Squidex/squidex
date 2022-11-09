// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public sealed class UpdateRole : AppCommand
{
    public string Name { get; set; }

    public string[] Permissions { get; set; }

    public JsonObject Properties { get; set; }
}
