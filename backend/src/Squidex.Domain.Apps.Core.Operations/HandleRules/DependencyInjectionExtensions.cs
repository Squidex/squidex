// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRuleAction<TAction, THandler>(this IServiceCollection services) where THandler : class, IRuleActionHandler where TAction : RuleAction
    {
        services.AddSingleton<IRuleActionHandler, THandler>();
        services.AddSingleton(new RuleActionRegistration(typeof(TAction)));

        return services;
    }
}
