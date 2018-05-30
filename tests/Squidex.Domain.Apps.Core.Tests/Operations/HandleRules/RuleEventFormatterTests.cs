// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class RuleEventFormatterTests
    {
        private readonly JsonSerializer serializer = JsonSerializer.CreateDefault();
        private readonly MemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IUser user = A.Fake<IUser>();
        private readonly IRuleUrlGenerator urlGenerator = A.Fake<IRuleUrlGenerator>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly Guid contentId = Guid.NewGuid();
        private readonly RuleEventFormatter sut;

        public RuleEventFormatterTests()
        {
            A.CallTo(() => user.Email)
                .Returns("me@email.com");

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.SquidexDisplayName, "me") });

            sut = new RuleEventFormatter(serializer, urlGenerator, memoryCache, userResolver);
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
            var @event = new ContentCreated { AppId = appId };

            var result = sut.ToRouteData(AsEnvelope(@event));

            Assert.True(result is JObject);
        }

        [Fact]
        public void Should_create_route_data_from_event()
        {
            var @event = new ContentCreated { AppId = appId };

            var result = sut.ToRouteData(AsEnvelope(@event), "MyEventName");

            Assert.Equal("MyEventName", result["type"]);
        }

        [Fact]
        public async Task Should_replace_app_information_from_event()
        {
            var @event = new ContentCreated { AppId = appId };

            var result = await sut.FormatStringAsync("Name $APP_NAME has id $APP_ID", AsEnvelope(@event));

            Assert.Equal($"Name my-app has id {appId.Id}", result);
        }

        [Fact]
        public async Task Should_replace_schema_information_from_event()
        {
            var @event = new ContentCreated { SchemaId = schemaId };

            var result = await sut.FormatStringAsync("Name $SCHEMA_NAME has id $SCHEMA_ID", AsEnvelope(@event));

            Assert.Equal($"Name my-schema has id {schemaId.Id}", result);
        }

        [Fact]
        public async Task Should_replace_timestamp_information_from_event()
        {
            var now = DateTime.UtcNow;

            var envelope = AsEnvelope(new ContentCreated()).SetTimestamp(Instant.FromDateTimeUtc(now));

            var result = await sut.FormatStringAsync("Date: $TIMESTAMP_DATE, Full: $TIMESTAMP_DATETIME", envelope);

            Assert.Equal($"Date: {now:yyyy-MM-dd}, Full: {now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Fact]
        public async Task Should_format_email_and_display_name_from_user()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Returns(user);

            var @event = new ContentCreated { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", AsEnvelope(@event));

            Assert.Equal($"From me (me@email.com)", result);
        }

        [Fact]
        public async Task Should_return_undefined_if_user_is_not_found()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Returns(Task.FromResult<IUser>(null));

            var @event = new ContentCreated { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", AsEnvelope(@event));

            Assert.Equal($"From UNDEFINED (UNDEFINED)", result);
        }

        [Fact]
        public async Task Should_return_undefined_if_user_failed_to_resolve()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Throws(new InvalidOperationException());

            var @event = new ContentCreated { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", AsEnvelope(@event));

            Assert.Equal($"From UNDEFINED (UNDEFINED)", result);
        }

        [Fact]
        public async Task Should_format_email_and_display_name_from_client()
        {
            var @event = new ContentCreated { Actor = new RefToken("client", "android") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", AsEnvelope(@event));

            Assert.Equal($"From client:android (client:android)", result);
        }

        [Fact]
        public async Task Should_replace_content_url_from_event()
        {
            var url = "http://content";

            A.CallTo(() => urlGenerator.GenerateContentUIUrl(appId, schemaId, contentId))
                .Returns(url);

            var @event = new ContentCreated { AppId = appId, ContentId = contentId, SchemaId = schemaId };

            var result = await sut.FormatStringAsync("Go to $CONTENT_URL", AsEnvelope(@event));

            Assert.Equal($"Go to {url}", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_field_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.country.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_partition_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_array_item_not_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JArray()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de.10", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_property_not_found()
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

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de.Name", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_plain_value_when_found()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_plain_value_when_found_from_update_event()
        {
            var @event = new ContentUpdated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_null()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateNull()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_undefined()
        {
            var @event = new ContentCreated
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateUndefined()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_string_when_object()
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

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", AsEnvelope(@event));

            Assert.Equal(JObject.FromObject(new { name = "Berlin" }).ToString(Formatting.Indented), result);
        }

        [Fact]
        public async Task Should_return_plain_value_from_array_when_found()
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

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv.0", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_plain_value_from_object_when_found()
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

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv.name", AsEnvelope(@event));

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_format_content_actions_when_found()
        {
            Assert.Equal("created", await sut.FormatStringAsync("$CONTENT_ACTION", AsEnvelope(new ContentCreated())));
            Assert.Equal("updated", await sut.FormatStringAsync("$CONTENT_ACTION", AsEnvelope(new ContentUpdated())));
            Assert.Equal("deleted", await sut.FormatStringAsync("$CONTENT_ACTION", AsEnvelope(new ContentDeleted())));

            Assert.Equal("set to archived", await sut.FormatStringAsync("$CONTENT_ACTION", AsEnvelope(new ContentStatusChanged { Status = Status.Archived })));
        }

        private static Envelope<AppEvent> AsEnvelope<T>(T @event) where T : AppEvent
        {
            return Envelope.Create<AppEvent>(@event).To<AppEvent>();
        }
    }
}
