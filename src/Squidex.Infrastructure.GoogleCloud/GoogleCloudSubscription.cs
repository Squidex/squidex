// ==========================================================================
//  GoogleCloudSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.GoogleCloud
{
    public class GoogleCloudSubscription : DisposableObject
    {
        private static readonly Guid InstanceId = Guid.NewGuid();
        private const string EmptyData = "Empty";
        private readonly Subject<string> subject = new Subject<string>();
        private readonly PublisherClient publisher = PublisherClient.Create();
        private readonly TopicName topicName;
        private readonly ILogger<GoogleCloudPubSub> logger;
        private readonly Task pullTask;
        private readonly CancellationTokenSource completionToken = new CancellationTokenSource();

        public GoogleCloudSubscription(TopicName topicName, ILogger<GoogleCloudPubSub> logger)
        {
            this.topicName = topicName;

            this.logger = logger;

            pullTask = PullAsync();
        }

        protected override void DisposeObject(bool disposing)
        {
            completionToken.Cancel();

            pullTask.Wait();
        }

        public void Publish(string token, bool notifySelf)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = EmptyData;
                }

                var message = new PubsubMessage
                {
                    Attributes =
                    {
                        { "Sender", (notifySelf ? Guid.Empty : InstanceId).ToString() }
                    },
                    Data = ByteString.CopyFromUtf8(token)
                };

                publisher.Publish(topicName, new [] { message });
            }
            catch (Exception ex)
            {
                logger.LogError(InfrastructureErrors.InvalidatingReceivedFailed, ex, "Failed to send invalidation message {0}", token);
            }
        }

        private async Task PullAsync()
        {
            var subscriber = SubscriberClient.Create();
            var subscriptionName = new SubscriptionName(topicName.ProjectId, "squidex-" + Guid.NewGuid());

            await subscriber.CreateSubscriptionAsync(subscriptionName, topicName, null, 60);

            try
            {
                while (!completionToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await subscriber.PullAsync(subscriptionName, false, int.MaxValue, completionToken.Token);

                        foreach (var receivedMessage in response.ReceivedMessages)
                        {
                            var token = receivedMessage.Message.Data.ToString(Encoding.UTF8);

                            Guid sender;

                            if (!receivedMessage.Message.Attributes.ContainsKey("Sender") || !Guid.TryParse(receivedMessage.Message.Attributes["Sender"], out sender))
                            {
                                return;
                            }

                            if (sender != InstanceId)
                            {
                                subject.OnNext(token);
                            }
                        }

                        await subscriber.AcknowledgeAsync(subscriptionName, response.ReceivedMessages.Select(m => m.AckId));
                    }
                    catch (RpcException e)
                    {
                        if (e.Status.StatusCode == StatusCode.DeadlineExceeded)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Pull process has been cancelled.");
            }
            finally
            {
                await subscriber.DeleteSubscriptionAsync(subscriptionName);
            }
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return subject.Subscribe(handler);
        }
    }
}
