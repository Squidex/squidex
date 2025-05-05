// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.States;

namespace Microsoft.EntityFrameworkCore;

public static class EFRuleBuilder
{
    public static void UseRules(this ModelBuilder builder, IJsonSerializer jsonSerializer, string? jsonColumn)
    {
        builder.UseSnapshot<Rule, EFRuleEntity>(jsonSerializer, jsonColumn, b =>
        {
            b.Property(x => x.IndexedAppId).AsString();
            b.Property(x => x.IndexedId).AsString();
        });
    }
}
