// ==========================================================================
//  GoogleCloudInvalidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.GoogleCloud
{
    public class GoogleCloudPubSub : DisposableObject, IPubSub, IExternalSystem
    {
        private readonly ProjectName projectName;
        private readonly ILogger<GoogleCloudPubSub> logger;
        private readonly ConcurrentDictionary<string, GoogleCloudSubscription> subscriptions = new ConcurrentDictionary<string, GoogleCloudSubscription>();
        private readonly PublisherClient publisher = PublisherClient.Create();

        public GoogleCloudPubSub(ProjectName projectName, ILogger<GoogleCloudPubSub> logger)
        {
            Guard.NotNull(projectName, nameof(projectName));
            Guard.NotNull(logger, nameof(logger));

            this.projectName = projectName;

            this.logger = logger;
        }

        protected override void DisposeObject(bool disposing)
        {
            foreach (var subscription in subscriptions.Values)
            {
                subscription.Dispose();
            }
        }

        public void Connect()
        {
            try
            {
                try
                {
                    publisher.CreateTopic(new TopicName(projectName.ProjectId, "connection-test"));
                }
                catch (RpcException e)
                {
                    if (e.Status.StatusCode != StatusCode.AlreadyExists)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"GoogleCloud connection failed to connect to project {projectName.ProjectId}", ex);
            }
        }

        public void Publish(string channelName, string token, bool notifySelf)
        {
            Guard.NotNull(channelName, nameof(channelName));

            subscriptions.GetOrAdd(channelName, Create).Publish(token, notifySelf);
        }

        public IDisposable Subscribe(string channelName, Action<string> handler)
        {
            Guard.NotNull(channelName, nameof(channelName));

            return subscriptions.GetOrAdd(channelName, Create).Subscribe(handler);
        }

        private GoogleCloudSubscription Create(string channelName)
        {
            var topicName = new TopicName(projectName.ProjectId, channelName);

            try
            {
                publisher.CreateTopic(topicName);
            }
            catch (RpcException e)
            {
                if (e.Status.StatusCode != StatusCode.AlreadyExists)
                {
                    throw;
                }
            }

            return new GoogleCloudSubscription(topicName, logger);
        }
    }
}
