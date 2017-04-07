// ==========================================================================
//  RabbitMqEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.RabbitMq
{
    public sealed class RabbitMqEventConsumer : DisposableObjectBase, IExternalSystem, IEventConsumer
    {
        private readonly string exchange;
        private readonly string streamFilter;
        private readonly ConnectionFactory connectionFactory;
        private readonly Lazy<IConnection> connection;
        private readonly Lazy<IModel> channel;

        public string Name
        {
            get { return GetType().Name; }
        }

        public string StreamFilter
        {
            get { return streamFilter; }
        }

        public RabbitMqEventConsumer(string uri, string exchange, string streamFilter)
        {
            Guard.NotNullOrEmpty(uri, nameof(uri));
            Guard.NotNullOrEmpty(exchange, nameof(exchange));

            connectionFactory = new ConnectionFactory { Uri = uri };

            connection = new Lazy<IConnection>(connectionFactory.CreateConnection);
            channel = new Lazy<IModel>(() => connection.Value.CreateModel());

            this.exchange = exchange;

            this.streamFilter = streamFilter;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (connection.IsValueCreated)
            {
                connection.Value.Close();
                connection.Value.Dispose();
            }
        }

        public void Connect()
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

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            var jsonString = JsonConvert.SerializeObject(@event);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            channel.Value.BasicPublish(exchange, string.Empty, null, jsonBytes);

            return TaskHelper.Done;
        }
    }
}
