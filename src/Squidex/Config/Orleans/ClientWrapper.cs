// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Squidex.Domain.Apps.Entities;
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
                .UseStaticClustering(options =>
                {
                    options.Gateways.Add(new IPEndPoint(ConfigUtilities.SiloAddress, 40000).ToGatewayUri());
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "squidex";
                })
                .ConfigureApplicationParts(builder =>
                {
                    builder.AddApplicationPart(SquidexEntities.Assembly);
                    builder.AddApplicationPart(SquidexInfrastructure.Assembly);
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
