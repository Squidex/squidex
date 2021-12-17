// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Hosting;
using OrleansHostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Squidex.Config.Orleans
{
    public class CustomSiloBuilder : OrleansHostBuilderContext, ISiloBuilder
    {
        private readonly List<Action<OrleansHostBuilderContext, IServiceCollection>> actions = new List<Action<OrleansHostBuilderContext, IServiceCollection>>();

        public CustomSiloBuilder(IConfiguration configuration, IWebHostEnvironment environment)
            : base(new Dictionary<object, object>())
        {
            Configuration = configuration;

            HostingEnvironment = environment;

            this.ConfigureDefaults();
        }

        public ISiloBuilder ConfigureServices(Action<OrleansHostBuilderContext, IServiceCollection> configureDelegate)
        {
            actions.Add(configureDelegate);

            return this;
        }

        public void Build(IServiceCollection serviceCollection)
        {
            foreach (var action in actions)
            {
                action(this, serviceCollection);
            }
        }
    }
}
