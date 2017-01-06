// ==========================================================================
//  EventChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.RabbitMq
{
    public sealed class RabbitMqEventChannel : DisposableObject, IEventPublisher, IEventStream
    {
        private const string Exchange = "Squidex";
        private readonly Lazy<IModel> currentChannel;

        public RabbitMqEventChannel(IConnectionFactory connectionFactory)
        {
            Guard.NotNull(connectionFactory, nameof(connectionFactory));

            currentChannel = new Lazy<IModel>(() => Connect(connectionFactory));
        }

        protected override void DisposeObject(bool disposing)
        {
            if (currentChannel.IsValueCreated)
            {
                currentChannel.Value.Dispose();
            }
        }

        public void Publish(EventData events)
        {
            ThrowIfDisposed();

            var channel = currentChannel.Value;

            channel.BasicPublish(Exchange, string.Empty, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(events)));
        }

        public void Connect(string queueName, Action<EventData> received)
        {
            ThrowIfDisposed();

            var channel = currentChannel.Value;

            queueName = $"{queueName}_{Environment.MachineName}";

            channel.QueueDeclare(queueName, true, false, false);
            channel.QueueBind(queueName, Exchange, string.Empty);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, e) =>
            {
                var eventData = JsonConvert.DeserializeObject<EventData>(Encoding.UTF8.GetString(e.Body));

                received(eventData);
            };

            channel.BasicConsume(queueName, false, consumer);
        }

        private static IModel Connect(IConnectionFactory connectionFactory)
        {
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true);

            return channel;
        }
    }
}
