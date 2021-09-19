// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class RabbitMqEventConsumer : DisposableObjectBase, IInitializable, IEventConsumer
    {
        private readonly IJsonSerializer jsonSerializer;
        private readonly string eventPublisherName;
        private readonly string exchange;
        private readonly string eventsFilter;
        private readonly ConnectionFactory connectionFactory;
        private readonly Lazy<IConnection> connection;
        private readonly Lazy<IModel> channel;

        public string Name
        {
            get => eventPublisherName;
        }

        public string EventsFilter
        {
            get => eventsFilter;
        }

        public RabbitMqEventConsumer(IJsonSerializer jsonSerializer, string eventPublisherName, string uri, string exchange, string eventsFilter)
        {
            connectionFactory = new ConnectionFactory { Uri = new Uri(uri, UriKind.Absolute) };
            connection = new Lazy<IConnection>(connectionFactory.CreateConnection);
            channel = new Lazy<IModel>(connection.Value.CreateModel);

            this.exchange = exchange;
            this.eventsFilter = eventsFilter;
            this.eventPublisherName = eventPublisherName;
            this.jsonSerializer = jsonSerializer;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (connection.IsValueCreated)
            {
                connection.Value.Close();
                connection.Value.Dispose();
            }
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            try
            {
                var currentConnection = connection.Value;

                if (!currentConnection.IsOpen)
                {
                    var error = new ConfigurationError($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}.");

                    throw new ConfigurationException(error);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var error = new ConfigurationError($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}.");

                throw new ConfigurationException(error, ex);
            }
        }

        public Task On(Envelope<IEvent> @event)
        {
            if (@event.Headers.Restored())
            {
                return Task.CompletedTask;
            }

            var jsonString = jsonSerializer.Serialize(@event);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            channel.Value.BasicPublish(exchange, string.Empty, null, jsonBytes);

            return Task.CompletedTask;
        }
    }
}
