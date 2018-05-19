// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;
using Orleans.Configuration;
using Squidex.Config.Domain;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public sealed class ClientWrapper : DisposableObjectBase, IInitializable, IDisposable
    {
        public IClusterClient Client { get; }

        public ClientWrapper()
        {
            Client = new ClientBuilder()
                .UseDashboard()
                .UseLocalhostClustering(40000)
                .Configure<ClusterOptions>(options =>
                {
                    options.Configure();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddMySerializers();
                })
                .ConfigureApplicationParts(builder =>
                {
                    builder.AddMyParts();
                })
                .Build();
        }

        public void Initialize()
        {
            Client.Connect().Wait();
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                Client.Close().Wait();
            }
        }
    }
}
