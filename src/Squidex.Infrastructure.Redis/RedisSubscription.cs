// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Reactive.Subjects;
using Newtonsoft.Json;
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
        private readonly ISemanticLog log;
        private readonly string channelName;

        private sealed class Envelope
        {
            public T Payload;

            public Guid Sender;
        }

        public RedisSubscription(ISubscriber subscriber, string channelName, ISemanticLog log)
        {
            this.log = log;

            this.subscriber = subscriber;
            this.subscriber.Subscribe(channelName, (channel, value) => HandleMessage(value));

            this.channelName = channelName;
        }

        public void Publish(object value, bool notifySelf)
        {
            try
            {
                var senderId = notifySelf ? Guid.Empty : selfId;

                var envelope = JsonConvert.SerializeObject(new Envelope { Sender = senderId, Payload = (T)value });

                subscriber.Publish(channelName, envelope);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "PublishRedisMessage")
                    .WriteProperty("status", "Failed")
                    .WriteProperty("channel", channelName));
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

                var envelope = JsonConvert.DeserializeObject<Envelope>(value);

                if (envelope.Sender != selfId)
                {
                    subject.OnNext(envelope.Payload);

                    log.LogDebug(w => w
                        .WriteProperty("action", "ReceiveRedisMessage")
                        .WriteProperty("channel", channelName)
                        .WriteProperty("status", "Received"));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "ReceiveRedisMessage")
                    .WriteProperty("channel", channelName)
                    .WriteProperty("status", "Failed"));
            }
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            return subject.Subscribe(handler);
        }
    }
}
