﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;

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
            get { return eventPublisherName; }
        }

        public string EventsFilter
        {
            get { return eventsFilter; }
        }

        public RabbitMqEventConsumer(IJsonSerializer jsonSerializer, string eventPublisherName, string uri, string exchange, string eventsFilter)
        {
            Guard.NotNullOrEmpty(uri);
            Guard.NotNullOrEmpty(eventPublisherName);
            Guard.NotNullOrEmpty(exchange);
            Guard.NotNull(jsonSerializer);

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

        public Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                var currentConnection = connection.Value;

                if (!currentConnection.IsOpen)
                {
                    throw new ConfigurationException($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}");
                }

                return TaskHelper.Done;
            }
            catch (Exception e)
            {
                throw new ConfigurationException($"RabbitMq event bus failed to connect to {connectionFactory.Endpoint}", e);
            }
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public Task On(Envelope<IEvent> @event)
        {
            var jsonString = jsonSerializer.Serialize(@event);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            channel.Value.BasicPublish(exchange, string.Empty, null, jsonBytes);

            return TaskHelper.Done;
        }
    }
}
