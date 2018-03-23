// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class RuleEventFormatterTests
    {
        private readonly JsonSerializer serializer = JsonSerializer.CreateDefault();
        private readonly RuleEventFormatter sut;

        public RuleEventFormatterTests()
        {
            sut = new RuleEventFormatter(serializer);
        }

        [Fact]
        public void Should_serialize_object_to_json()
        {
            var result = sut.ToRouteData(new { Value = 1 });

            Assert.True(result is JObject);
        }

        [Fact]
        public void Should_create_route_data()
        {
            var appId = Guid.NewGuid();

            var @event = new ContentCreated
            {
                AppId = new NamedId<Guid>(appId, "my-app")
            };

            var result = sut.ToRouteData(AsEnvelope(@event));

            Assert.True(result is JObject);
        }

        [Fact]
        public void Should_create_route_data_from_event()
        {
            var appId = Guid.NewGuid();

            var @event = new ContentCreated
            {
                AppId = new NamedId<Guid>(appId, "my-app")
            };

            var result = sut.ToRouteData(AsEnvelope(@event), "MyEventName");

            Assert.Equal("MyEventName", result["type"]);
        }

        [Fact]
        public void Should_replace_app_information_from_event()
        {
            var appId = Guid.NewGuid();

            var @event = new ContentCreated
            {
                AppId = new NamedId<Guid>(appId, "my-app")
            };

            var result = sut.FormatString("Name $APP_NAME has id $APP_ID", AsEnvelope(@event));

            Assert.Equal($"Name my-app has id {appId}", result);
        }

        [Fact]
        public void Should_replace_schema_information_from_event()
        {
            var schemaId = Guid.NewGuid();

            var @event = new ContentCreated
            {
                SchemaId = new NamedId<Guid>(schemaId, "my-schema")
            };

            var result = sut.FormatString("Name $SCHEMA_NAME has id $SCHEMA_ID", AsEnvelope(@event));

            Assert.Equal($"Name my-schema has id {schemaId}", result);
        }

        [Fact]
        public void Should_replace_timestamp_information_from_event()
        {
            var now = DateTime.UtcNow;

            var envelope = Envelope.Create(new ContentCreated()).To<AppEvent>().SetTimestamp(Instant.FromDateTimeUtc(now));

            var result = sut.FormatString("Date: $TIMESTAMP_DATE, Full: $TIMESTAMP_DATETIME", envelope);

            Assert.Equal($"Date: {now:yyyy-MM-dd}, Full: {now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Fact]
        public void Should_return_undefined_when_field_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = sut.FormatString("$CONTENT_DATA.country.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_undefined_when_partition_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.de", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_undefined_when_array_item_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JArray()))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.de.10", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_undefined_when_property_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.de.Name", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_plain_value_when_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public void Should_return_plain_value_when_found_from_update_event()
        {
            var @event = new ContentUpdated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public void Should_return_undefined_when_null()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateNull()))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_undefined_when_undefined()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateUndefined()))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public void Should_return_string_when_object()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal(JObject.FromObject(new { name = "Berlin" }).ToString(Formatting.Indented), result);
        }

        [Fact]
        public void Should_return_plain_value_from_array_when_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JArray(
                                    "Berlin")))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv.0", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public void Should_return_plain_value_from_object_when_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = sut.FormatString("$CONTENT_DATA.city.iv.name", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public void Should_format_content_action_for_created_when_found()
        {
            Assert.Equal("created", sut.FormatString("$CONTENT_ACTION", AsEnvelope(new ContentCreated())));
            Assert.Equal("updated", sut.FormatString("$CONTENT_ACTION", AsEnvelope(new ContentUpdated())));
            Assert.Equal("deleted", sut.FormatString("$CONTENT_ACTION", AsEnvelope(new ContentDeleted())));

            Assert.Equal("set to archived", sut.FormatString("$CONTENT_ACTION", AsEnvelope(new ContentStatusChanged { Status = Status.Archived })));
        }

        private static Envelope<AppEvent> AsEnvelope<T>(T @event) where T : AppEvent
        {
            return Envelope.Create<AppEvent>(@event).To<AppEvent>();
        }
    }
}
