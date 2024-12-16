// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Squidex.Extensions.Actions.AzureQueue;

internal sealed class AzureQueuePool : ClientPool<(string ConnectionString, string QueueName), CloudQueue>
{
    public AzureQueuePool()
        : base(CreateClient)
    {
    }

    private static CloudQueue CreateClient((string ConnectionString, string QueueName) key)
    {
        var storageAccount = CloudStorageAccount.Parse(key.ConnectionString);

        var queueClient = storageAccount.CreateCloudQueueClient();
        var queueRef = queueClient.GetQueueReference(key.QueueName);

        return queueRef;
    }
}
