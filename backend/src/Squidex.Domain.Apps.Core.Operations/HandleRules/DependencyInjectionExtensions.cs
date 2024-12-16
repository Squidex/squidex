// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddFlowStep<T>(this IServiceCollection services) where T : IFlowStep
    {
        services.Configure<FlowOptions>(options =>
        {
            options.Steps.Add(typeof(T));
        });

        return services;
    }
}
