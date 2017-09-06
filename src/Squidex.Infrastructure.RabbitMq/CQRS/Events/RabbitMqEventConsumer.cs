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
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class RabbitMqEventConsumer : DisposableObjectBase, IExternalSystem, IEventConsumer
    {
        private readonly JsonSerializerSettings serializerSettings;
        private readonly string eventPublisherName;
        private readonly string exchange;
        private readonly string eventsFilter;
        private readonly ConnectionFactory connectionFactory;
        private readonly Lazy<IConnection> connection;
        private readonly Lazy<IModel> channel;

        public string Name
        {
            get { return eventPublisherName; }
        }

        public string EventsFilter
        {
            get { return eventsFilter; }
        }

        public RabbitMqEventConsumer(JsonSerializerSettings serializerSettings, string eventPublisherName, string uri, string exchange, string eventsFilter)
        {
            Guard.NotNullOrEmpty(uri, nameof(uri));
            Guard.NotNullOrEmpty(eventPublisherName, nameof(eventPublisherName));
            Guard.NotNullOrEmpty(exchange, nameof(exchange));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            connectionFactory = new ConnectionFactory { Uri = new Uri(uri, UriKind.Absolute) };
            connection = new Lazy<IConnection>(connectionFactory.CreateConnection);
            channel = new Lazy<IModel>(() => connection.Value.CreateModel());

            this.exchange = exchange;
            this.eventsFilter = eventsFilter;
            this.eventPublisherName = eventPublisherName;
            this.serializerSettings = serializerSettings;
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
            var jsonString = JsonConvert.SerializeObject(@event, serializerSettings);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            channel.Value.BasicPublish(exchange, string.Empty, null, jsonBytes);

            return TaskHelper.Done;
        }
    }
}
