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
    public sealed class RabbitMqEventBus : DisposableObject, IEventPublisher, IEventStream, IExternalSystem
    {
        private readonly bool isPersistent;
        private readonly string queueName;
        private const string Exchange = "Squidex";
        private readonly ConnectionFactory connectionFactory;
        private readonly Lazy<IConnection> connection;
        private readonly Lazy<IModel> channel;
        private EventingBasicConsumer consumer;

        public RabbitMqEventBus(ConnectionFactory connectionFactory, bool isPersistent, string queueName)
        {
            Guard.NotNull(connectionFactory, nameof(connectionFactory));

            this.queueName = queueName;

            this.connectionFactory = connectionFactory;

            connection = new Lazy<IConnection>(connectionFactory.CreateConnection);
            channel = new Lazy<IModel>(() => CreateChannel(connection.Value));

            this.isPersistent = isPersistent;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (connection.IsValueCreated)
            {
                connection.Value.Close();
                connection.Value.Dispose();
            }
        }

        public void Publish(EventData eventData)
        {
            ThrowIfDisposed();
            
            channel.Value.BasicPublish(Exchange, string.Empty, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventData)));
        }

        public void CheckConnection()
        {
            try
            {
                var currentConnection = connection.Value;

                if (!currentConnection.IsOpen)
                {
                    throw new ConfigurationException($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}");
                }
            }
            catch (Exception e)
            {
                throw new ConfigurationException($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}", e);
            }
        }

        public void Connect(string queuePrefix, Action<EventData> received)
        {
            ThrowIfDisposed();
            ThrowIfConnected();

            lock (connection)
            {
                var currentChannel = channel.Value;

                ThrowIfConnected();
                
                var fullQueueName = $"{queuePrefix}_";

                if (!string.IsNullOrWhiteSpace(queueName))
                {
                    fullQueueName += queueName;
                }
                else
                {
                    fullQueueName += Environment.MachineName;
                }

                currentChannel.QueueDeclare(fullQueueName, isPersistent, false, !isPersistent);
                currentChannel.QueueBind(fullQueueName, Exchange, string.Empty);

                consumer = new EventingBasicConsumer(currentChannel);

                consumer.Received += (model, e) =>
                {
                    var eventData = JsonConvert.DeserializeObject<EventData>(Encoding.UTF8.GetString(e.Body));

                    received(eventData);
                };

                currentChannel.BasicConsume(fullQueueName, true, consumer);
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
