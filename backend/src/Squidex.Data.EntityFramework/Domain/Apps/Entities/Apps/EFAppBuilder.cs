// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Microsoft.EntityFrameworkCore;

public static class EFAppBuilder
{
    public static void UseApps(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<App, EFAppEntity>(jsonSerializer, jsonColumn, b =>
        {
            b.Property(x => x.IndexedTeamId).AsString();
        });
    }
}
