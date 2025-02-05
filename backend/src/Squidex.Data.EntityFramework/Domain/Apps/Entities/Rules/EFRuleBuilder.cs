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

        builder.Entity<EFRuleEventEntity>(b =>
        {
            b.Property(x => x.Id).AsString();
            b.Property(x => x.AppId).AsString();
            b.Property(x => x.Created).AsDateTimeOffset();
            b.Property(x => x.Expires).AsDateTimeOffset();
            b.Property(x => x.Job).AsJsonString(jsonSerializer, jsonColumn);
            b.Property(x => x.JobResult).AsString();
            b.Property(x => x.LastModified).AsDateTimeOffset();
            b.Property(x => x.NextAttempt).AsDateTimeOffset();
            b.Property(x => x.Result).AsString();
            b.Property(x => x.RuleId).AsString();
        });
    }
}
