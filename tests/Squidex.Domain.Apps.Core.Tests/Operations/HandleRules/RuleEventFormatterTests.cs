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
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;
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
            var result = sut.ToPayload(new { Value = 1 });

            Assert.True(result is JObject);
        }

        [Fact]
        public void Should_create_route_data()
        {
            var @event = new EnrichedContentEvent { AppId = appId };

            var result = sut.ToPayload(@event);

            Assert.True(result is JObject);
        }

        [Fact]
        public void Should_create_route_data_from_event()
        {
            var @event = new EnrichedContentEvent { AppId = appId, Name = "MyEventName" };

            var result = sut.ToPayload(@event);

            Assert.Equal("MyEventName", result["type"]);
        }

        [Fact]
        public async Task Should_replace_app_information_from_event()
        {
            var @event = new EnrichedContentEvent { AppId = appId };

            var result = await sut.FormatStringAsync("Name $APP_NAME has id $APP_ID", @event);

            Assert.Equal($"Name my-app has id {appId.Id}", result);
        }

        [Fact]
        public async Task Should_replace_schema_information_from_event()
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaId };

            var result = await sut.FormatStringAsync("Name $SCHEMA_NAME has id $SCHEMA_ID", @event);

            Assert.Equal($"Name my-schema has id {schemaId.Id}", result);
        }

        [Fact]
        public async Task Should_replace_timestamp_information_from_event()
        {
            var now = DateTime.UtcNow;

            var envelope = new EnrichedContentEvent { Timestamp = Instant.FromDateTimeUtc(now) };

            var result = await sut.FormatStringAsync("Date: $TIMESTAMP_DATE, Full: $TIMESTAMP_DATETIME", envelope);

            Assert.Equal($"Date: {now:yyyy-MM-dd}, Full: {now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Fact]
        public async Task Should_format_email_and_display_name_from_user()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Returns(user);

            var @event = new EnrichedContentEvent { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", @event);

            Assert.Equal($"From me (me@email.com)", result);
        }

        [Fact]
        public async Task Should_return_undefined_if_user_is_not_found()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Returns(Task.FromResult<IUser>(null));

            var @event = new EnrichedContentEvent { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", @event);

            Assert.Equal($"From UNDEFINED (UNDEFINED)", result);
        }

        [Fact]
        public async Task Should_return_undefined_if_user_failed_to_resolve()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync("123"))
                .Throws(new InvalidOperationException());

            var @event = new EnrichedContentEvent { Actor = new RefToken("subject", "123") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", @event);

            Assert.Equal($"From UNDEFINED (UNDEFINED)", result);
        }

        [Fact]
        public async Task Should_format_email_and_display_name_from_client()
        {
            var @event = new EnrichedContentEvent { Actor = new RefToken("client", "android") };

            var result = await sut.FormatStringAsync("From $USER_NAME ($USER_EMAIL)", @event);

            Assert.Equal($"From client:android (client:android)", result);
        }

        [Fact]
        public async Task Should_replace_content_url_from_event()
        {
            var url = "http://content";

            A.CallTo(() => urlGenerator.GenerateContentUIUrl(appId, schemaId, contentId))
                .Returns(url);

            var @event = new EnrichedContentEvent { AppId = appId, Id = contentId, SchemaId = schemaId };

            var result = await sut.FormatStringAsync("Go to $CONTENT_URL", @event);

            Assert.Equal($"Go to {url}", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_field_not_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.country.iv", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_partition_not_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_array_item_not_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JArray()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de.10", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_property_not_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.de.Name", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_plain_value_when_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", @event);

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_plain_value_when_found_from_update_event()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", @event);

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_null()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateNull()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_undefined_when_undefined()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", JValue.CreateUndefined()))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", @event);

            Assert.Equal("UNDEFINED", result);
        }

        [Fact]
        public async Task Should_return_string_when_object()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv", @event);

            Assert.Equal(JObject.FromObject(new { name = "Berlin" }).ToString(Formatting.Indented), result);
        }

        [Fact]
        public async Task Should_return_plain_value_from_array_when_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JArray(
                                    "Berlin")))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv.0", @event);

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_return_plain_value_from_object_when_found()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", new JObject(
                                    new JProperty("name", "Berlin"))))
            };

            var result = await sut.FormatStringAsync("$CONTENT_DATA.city.iv.name", @event);

            Assert.Equal("Berlin", result);
        }

        [Fact]
        public async Task Should_format_content_actions_when_found()
        {
            Assert.Equal("created", await sut.FormatStringAsync("$CONTENT_ACTION", new EnrichedContentEvent { Action = EnrichedContentEventAction.Created }));
            Assert.Equal("updated", await sut.FormatStringAsync("$CONTENT_ACTION", new EnrichedContentEvent { Action = EnrichedContentEventAction.Updated }));
            Assert.Equal("deleted", await sut.FormatStringAsync("$CONTENT_ACTION", new EnrichedContentEvent { Action = EnrichedContentEventAction.Deleted }));
            Assert.Equal("archived", await sut.FormatStringAsync("$CONTENT_ACTION", new EnrichedContentEvent { Action = EnrichedContentEventAction.Archived }));
        }
    }
}
