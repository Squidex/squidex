// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Azure.SignalR.Management;

namespace Squidex.Extensions.Actions.SignalR;

internal sealed class SignalRClientPool : ClientPool<(string ConnectionString, string HubName), ServiceManager>
{
    public SignalRClientPool()
        : base(CreateClient)
    {
    }

    private static ServiceManager CreateClient((string ConnectionString, string HubName) key)
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(option =>
            {
                option.ConnectionString = key.ConnectionString;
                option.ServiceTransportType = ServiceTransportType.Transient;
            })
            .BuildServiceManager();

        return serviceManager;
    }
}
