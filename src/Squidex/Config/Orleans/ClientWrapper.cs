// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Orleans.Runtime.Configuration;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public sealed class ClientWrapper : IInitializable, IDisposable
    {
        private readonly IClusterClient client;

        public IClusterClient Client
        {
            get { return client; }
        }

        public ClientWrapper()
        {
            client = new ClientBuilder()
                .UseConfiguration(ClientConfiguration.LocalhostSilo())
                .UseDashboard()
                .ConfigureApplicationParts(builder =>
                {
                    builder.AddApplicationPart(SquidexInfrastructure.Assembly);
                })
                .UseStaticGatewayListProvider(options =>
                {
                    options.Gateways.Add(new Uri("gwy.tcp://127.0.0.1:40000/0"));
                })
                .Build();
        }

        public void Initialize()
        {
            client.Connect().Wait();
        }

        public void Dispose()
        {
            client.Close().Wait();
        }
    }
}
