// ==========================================================================
//  RedisSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Reactive.Subjects;
using Squidex.Infrastructure.Log;
using StackExchange.Redis;

namespace Squidex.Infrastructure
{
    internal sealed class RedisSubscription
    {
        private static readonly Guid InstanceId = Guid.NewGuid();
        private readonly Subject<string> subject = new Subject<string>();
        private readonly ISubscriber subscriber;
        private readonly string channelName;
        private readonly ISemanticLog log;

        public RedisSubscription(ISubscriber subscriber, string channelName, ISemanticLog log)
        {
            this.log = log;

            this.subscriber = subscriber;
            this.subscriber.Subscribe(channelName, (channel, value) => HandleInvalidation(value));

            this.channelName = channelName;
        }

        public void Publish(string token, bool notifySelf)
        {
            try
            {
                var message = string.Join("#", (notifySelf ? Guid.Empty : InstanceId).ToString(), token);

                subscriber.Publish(channelName, message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "PublishRedisMessage")
                    .WriteProperty("state", "Failed")
                    .WriteProperty("token", token));
            }
        }

        private void HandleInvalidation(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var parts = value.Split('#');

                if (parts.Length < 1)
                {
                    return;
                }

                if (!Guid.TryParse(parts[0], out var sender))
                {
                    return;
                }

                if (sender != InstanceId)
                {
                    var token = string.Join("#", parts.Skip(1));

                    subject.OnNext(token);

                    log.LogDebug(w => w
                        .WriteProperty("action", "ReceiveRedisMessage")
                        .WriteProperty("channel", channelName)
                        .WriteProperty("token", token)
                        .WriteProperty("state", "Received"));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "ReceiveRedisMessage")
                    .WriteProperty("channel", channelName)
                    .WriteProperty("state", "Failed"));
            }
        }

        public IDisposable Subscribe(Action<string> handler)
        {
            return subject.Subscribe(handler);
        }
    }
}
