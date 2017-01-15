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
// ReSharper disable InvertIf

namespace Squidex.Infrastructure.RabbitMq
{
    public sealed class RabbitMqEventChannel : DisposableObject, IEventPublisher, IEventStream
    {
        private const string Exchange = "Squidex";
        private readonly IConnection connection;
        private readonly IModel channel;
        private EventingBasicConsumer consumer;

        public RabbitMqEventChannel(IConnectionFactory connectionFactory)
        {
            Guard.NotNull(connectionFactory, nameof(connectionFactory));

            connection = connectionFactory.CreateConnection();
            channel = CreateChannel(connection);
        }

        protected override void DisposeObject(bool disposing)
        {
            connection.Close();
            connection.Dispose();
        }

        public void Publish(EventData eventData)
        {
            ThrowIfDisposed();
            
            channel.BasicPublish(Exchange, string.Empty, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData)));
        }

        public void Connect(string queueName, Action<EventData> received)
        {
            ThrowIfDisposed();
            ThrowIfConnected();

            lock (connection)
            {
                ThrowIfConnected();
                
                queueName = $"{queueName}_{Environment.MachineName}";

                channel.QueueDeclare(queueName, true, false, false);
                channel.QueueBind(queueName, Exchange, string.Empty);

                consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, e) =>
                {
                    var eventData = JsonConvert.DeserializeObject<EventData>(Encoding.UTF8.GetString(e.Body));

                    received(eventData);
                };

                channel.BasicConsume(queueName, true, consumer);
            }
        }

        private static IModel CreateChannel(IConnection connection, bool declareExchange = true)
        {
            var channel = connection.CreateModel();

            if (declareExchange)
            {
                channel.ExchangeDeclare(Exchange, ExchangeType.Fanout, true);
            }

            return channel;
        }

        private void ThrowIfConnected()
        {
            if (consumer != null)
            {
                throw new InvalidOperationException("Already connected to channel.");
            }
        }
    }
}
