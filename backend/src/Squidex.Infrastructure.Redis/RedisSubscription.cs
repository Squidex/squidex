// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Reactive.Subjects;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using StackExchange.Redis;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure
{
    internal sealed class RedisSubscription<T>
    {
        private readonly Guid selfId = Guid.NewGuid();
        private readonly Subject<T> subject = new Subject<T>();
        private readonly ISubscriber subscriber;
        private readonly IJsonSerializer serializer;
        private readonly ISemanticLog log;
        private readonly string channelName;

        private sealed class Envelope
        {
            public T Payload;

            public Guid Sender;
        }

        public RedisSubscription(ISubscriber subscriber, IJsonSerializer serializer, string channelName, ISemanticLog log)
        {
            this.log = log;

            this.serializer = serializer;
            this.subscriber = subscriber;
            this.subscriber.Subscribe(channelName, (channel, value) => HandleMessage(value));

            this.channelName = channelName;
        }

        public void Publish(object value, bool notifySelf)
        {
            try
            {
                var senderId = notifySelf ? Guid.Empty : selfId;

                var envelope = serializer.Serialize(new Envelope { Sender = senderId, Payload = (T)value });

                subscriber.Publish(channelName, envelope);
            }
            catch (Exception ex)
            {
                log.LogError(ex, channelName, (logChannel, w) => w
                    .WriteProperty("action", "PublishRedisMessage")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("channel", logChannel));
            }
        }

        private void HandleMessage(string value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                var envelope = serializer.Deserialize<Envelope>(value);

                if (envelope.Sender != selfId)
                {
                    subject.OnNext(envelope.Payload);

                    log.LogDebug(channelName, (logChannel, w) => w
                        .WriteProperty("action", "ReceiveRedisMessage")
                        .WriteProperty("channel", logChannel)
                        .WriteProperty("status", "Received"));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, channelName, (logChannel, w) => w
                    .WriteProperty("action", "ReceiveRedisMessage")
                    .WriteProperty("channel", logChannel)
                    .WriteProperty("status", "Failed"));
            }
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            return subject.Subscribe(handler);
        }
    }
}
