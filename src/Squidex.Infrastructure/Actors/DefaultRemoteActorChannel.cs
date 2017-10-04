// ==========================================================================
//  DefaultRemoteActorChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Actors
{
    public sealed class DefaultRemoteActorChannel : IRemoteActorChannel
    {
        private static readonly string ChannelName = typeof(DefaultRemoteActorChannel).Name;
        private readonly IPubSub pubSub;
        private readonly JsonSerializer serializer;
        private readonly TypeNameRegistry typeNameRegistry;

        private sealed class Envelope
        {
            public string Recipient { get; set; }

            public string PayloadType { get; set; }

            public JToken Payload { get; set; }
        }

        public DefaultRemoteActorChannel(IPubSub pubSub, TypeNameRegistry typeNameRegistry, JsonSerializerSettings serializerSettings = null)
        {
            Guard.NotNull(pubSub, nameof(pubSub));
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            this.pubSub = pubSub;

            this.typeNameRegistry = typeNameRegistry;

            serializer = JsonSerializer.Create(serializerSettings ?? new JsonSerializerSettings());
        }

        public Task SendAsync(string recipient, object message)
        {
            Guard.NotNullOrEmpty(recipient, nameof(recipient));
            Guard.NotNull(message, nameof(message));

            var messageType = typeNameRegistry.GetName(message.GetType());
            var messageBody = WriteJson(message);

            var envelope = new Envelope { Recipient = recipient, Payload = messageBody, PayloadType = messageType };

            pubSub.Publish(ChannelName, JsonConvert.SerializeObject(envelope), true);

            return TaskHelper.Done;
        }

        public void Subscribe(string recipient, Action<object> handler)
        {
            Guard.NotNullOrEmpty(recipient, nameof(recipient));

            pubSub.Subscribe(ChannelName, json =>
            {
                var envelope = JsonConvert.DeserializeObject<Envelope>(json);

                if (string.Equals(envelope.Recipient, recipient, StringComparison.OrdinalIgnoreCase))
                {
                    var messageType = typeNameRegistry.GetType(envelope.PayloadType);
                    var messageBody = ReadJson(envelope.Payload, messageType);

                    handler?.Invoke(messageBody);
                }
            });
        }

        private object ReadJson(JToken token, Type type)
        {
            return token.ToObject(type, serializer);
        }

        private JToken WriteJson(object value)
        {
            return JToken.FromObject(value, serializer);
        }
    }
}
