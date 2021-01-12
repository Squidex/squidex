// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.ValidateContent;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Validation
{
    public sealed class CompositeUniqueValidatorPlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IValidatorsFactory, CompositeUniqueValidatorFactory>();
        }
    }
}
