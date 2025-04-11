// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Deprecated;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.DependencyInjection;

public static class RuleServiceExtensions
{
    [Obsolete("Use Flows")]
    public static void AddRuleAction<T>(this IServiceCollection services) where T : RuleAction
    {
        services.Configure<RulesOptions>(options =>
        {
            options.Actions.Add(typeof(T));
        });
    }
}
