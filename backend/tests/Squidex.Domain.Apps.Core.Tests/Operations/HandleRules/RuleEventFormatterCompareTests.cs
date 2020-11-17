﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.Extensions;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable CA2012 // Use ValueTasks correctly

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class RuleEventFormatterCompareTests
    {
        private readonly IUser user = A.Fake<IUser>();
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly DomainId assetId = DomainId.NewGuid();
        private readonly RuleEventFormatter sut;

        private class FakeContentResolver : IRuleEventFormatter
        {
            public (bool Match, ValueTask<string?>) Format(EnrichedEvent @event, object value, string[] path)
            {
                if (path[0] == "data" && value is JsonArray)
                {
                    return (true, GetValueAsync());
                }

                return default;
            }

            private static async ValueTask<string?> GetValueAsync()
            {
                await Task.Delay(5);

                return "Reference";
            }
        }

        public RuleEventFormatterCompareTests()
        {
            A.CallTo(() => urlGenerator.ContentUI(appId, schemaId, contentId))
                .Returns("content-url");

            A.CallTo(() => urlGenerator.AssetContent(appId, assetId.ToString()))
                .Returns("asset-content-url");

            A.CallTo(() => urlGenerator.AssetContent(appId, "file-name"))
                .Returns("asset-content-slug-url");

            A.CallTo(() => user.Id)
                .Returns("user123");

            A.CallTo(() => user.Email)
                .Returns("me@email.com");

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "me") });

            var formatters = new IRuleEventFormatter[]
            {
                new PredefinedPatternsFormatter(urlGenerator),
                new FakeContentResolver()
            };

            sut = new RuleEventFormatter(TestUtils.DefaultSerializer, formatters, BuildTemplateEngine(), BuildScriptEngine());
        }

        private FluidTemplateEngine BuildTemplateEngine()
        {
            var extensions = new IFluidExtension[]
            {
                new ContentFluidExtension(),
                new DateTimeFluidExtension(),
                new EventFluidExtensions(urlGenerator),
                new StringFluidExtension(),
                new StringWordsFluidExtension(),
                new UserFluidExtension()
            };

            return new FluidTemplateEngine(extensions);
        }

        private JintScriptEngine BuildScriptEngine()
        {
            var extensions = new IJintExtension[]
            {
                new DateTimeJintExtension(),
                new EventJintExtension(urlGenerator),
                new StringJintExtension(),
                new StringWordsJintExtension()
            };

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            return new JintScriptEngine(cache, extensions);
        }

        [Theory]
        [Expressions(
            "Name $APP_NAME has id $APP_ID",
            "Name ${EVENT_APPID.NAME} has id ${EVENT_APPID.ID}",
            "Name ${event.appId.name} has id ${event.appId.id}",
            "Name {{event.appId.name}} has id {{event.appId.id}}"
        )]
        public async Task Should_format_app_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { AppId = appId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Name my-app has id {appId.Id}", result);
        }

        [Theory]
        [Expressions(
            "Name $SCHEMA_NAME has id $SCHEMA_ID",
            "Name ${EVENT_SCHEMAID.NAME} has id ${EVENT_SCHEMAID.ID}",
            "Name ${event.schemaId.name} has id ${event.schemaId.id}",
            "Name {{event.schemaId.name}} has id {{event.schemaId.id}}"
        )]
        public async Task Should_format_schema_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Name my-schema has id {schemaId.Id}", result);
        }

        [Theory]
        [Expressions(
            "DateTime: $TIMESTAMP_DATETIME",
            null,
            "DateTime: ${formatDate(event.timestamp, 'yyyy-MM-dd-hh-mm-ss')}",
            "DateTime: {{event.timestamp | format_date: 'yyyy-MM-dd-hh-mm-ss'}}"
        )]
        public async Task Should_format_timestamp_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { Timestamp = now };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"DateTime: {now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Theory]
        [Expressions(
            "Date: $TIMESTAMP_DATE",
            null,
            "Date: ${formatDate(event.timestamp, 'yyyy-MM-dd')}",
            "Date: {{event.timestamp | format_date: 'yyyy-MM-dd'}}"
        )]
        public async Task Should_format_timestamp_date_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { Timestamp = now };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Date: {now:yyyy-MM-dd}", result);
        }

        [Theory]
        [Expressions(
            "From $MENTIONED_NAME ($MENTIONED_EMAIL, $MENTIONED_ID)",
            "From ${EVENT_MENTIONEDUSER.NAME} (${EVENT_MENTIONEDUSER.EMAIL}, ${EVENT_MENTIONEDUSER.ID})",
            "From ${event.mentionedUser.name} (${event.mentionedUser.email}, ${event.mentionedUser.id})",
            "From {{event.mentionedUser.name}} ({{event.mentionedUser.email}}, {{event.mentionedUser.id}})"
        )]
        public async Task Should_format_email_and_display_name_from_mentioned_user(string script)
        {
            var @event = new EnrichedCommentEvent { MentionedUser = user };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From me (me@email.com, user123)", result);
        }

        [Theory]
        [Expressions(
            "From $USER_NAME ($USER_EMAIL, $USER_ID)",
            "From ${EVENT_USER.NAME} (${EVENT_USER.EMAIL}, ${EVENT_USER.ID})",
            "From ${event.user.name} (${event.user.email}, ${event.user.id})",
            "From {{event.user.name}} ({{event.user.email}}, {{event.user.id}})"
        )]
        public async Task Should_format_email_and_display_name_from_user(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From me (me@email.com, user123)", result);
        }

        [Theory]
        [Expressions(
            "From $USER_NAME ($USER_EMAIL, $USER_ID)",
            "From ${EVENT_USER.NAME} (${EVENT_USER.EMAIL}, ${EVENT_USER.ID})",
            "From ${event.user.name} (${event.user.email}, ${event.user.id})",
            "From {{event.user.name | default: 'null'}} ({{event.user.email | default: 'null'}}, {{event.user.id | default: 'null'}})"
        )]
        public async Task Should_return_null_if_user_is_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From null (null, null)", result);
        }

        [Theory]
        [Expressions(
            "From $USER_NAME ($USER_EMAIL, $USER_ID)",
            "From ${EVENT_USER.NAME} (${EVENT_USER.EMAIL}, ${EVENT_USER.ID})",
            "From ${event.user.name} (${event.user.email}, ${event.user.id})",
            "From {{event.user.name}} ({{event.user.email}}, {{event.user.id}})"
        )]
        public async Task Should_format_email_and_display_name_from_client(string script)
        {
            var @event = new EnrichedContentEvent { User = new ClientUser(new RefToken(RefTokenType.Client, "android")) };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From client:android (client:android, android)", result);
        }

        [Theory]
        [Expressions(
            "Version: $ASSET_VERSION",
            "Version: ${EVENT_VERSION}",
            "Version: ${event.version}",
            "Version: {{event.version}}"
        )]
        public async Task Should_format_base_property(string script)
        {
            var @event = new EnrichedAssetEvent { Version = 13 };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Version: 13", result);
        }

        [Theory]
        [Expressions(
            "File: $ASSET_FILENAME",
            "File: ${EVENT_FILENAME}",
            "File: ${event.fileName}",
            "File: {{event.fileName}}"
        )]
        public async Task Should_format_asset_file_name_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { FileName = "my-file.png" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("File: my-file.png", result);
        }

        [Theory]
        [Expressions(
            "Type: $ASSSET_ASSETTYPE",
            "Type: ${EVENT_ASSETTYPE}",
            "Type: ${event.assetType}",
            "Type: {{event.assetType}}"
        )]
        public async Task Should_format_asset_asset_type_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { AssetType = AssetType.Audio };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Type: Audio", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_URL",
            null,
            "Download at ${assetContentUrl()}",
            "Download at {{event.id | assetContentUrl}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentUrl}})")]
        public async Task Should_format_asset_content_url_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { Id = assetId, AppId = appId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at asset-content-url", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_URL",
            null,
            "Download at ${assetContentUrl()}",
            "Download at {{event.id | assetContentUrl | default: 'null'}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentUrl | default: 'null'}})")]
        public async Task Should_return_null_when_asset_content_url_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at null", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_APP_URL",
            null,
            "Download at ${assetContentAppUrl()}",
            "Download at {{event.id | assetContentAppUrl | default: 'null'}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentAppUrl | default: 'null'}})")]
        public async Task Should_format_asset_content_app_url_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { AppId = appId, Id = assetId, FileName = "File Name" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at asset-content-url", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_APP_URL",
            null,
            "Download at ${assetContentAppUrl()}",
            "Download at {{event.id | assetContentAppUrl | default: 'null'}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentAppUrl | default: 'null'}})")]
        public async Task Should_return_null_when_asset_content_app_url_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at null", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_SLUG_URL",
            null,
            "Download at ${assetContentSlugUrl()}",
            "Download at {{event.fileName | assetContentSlugUrl | default: 'null'}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentSlugUrl | default: 'null'}})")]
        public async Task Should_format_asset_content_slug_url_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { AppId = appId, Id = assetId, FileName = "File Name" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at asset-content-slug-url", result);
        }

        [Theory]
        [Expressions(
            "Download at $ASSET_CONTENT_SLUG_URL",
            null,
            "Download at ${assetContentSlugUrl()}",
            "Download at {{event.id | assetContentSlugUrl | default: 'null'}}"
        )]
        [InlineData("Liquid(Download at {{event | assetContentSlugUrl | default: 'null'}})")]
        public async Task Should_return_null_when_asset_content_slug_url_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at null", result);
        }

        [Theory]
        [Expressions(
            "Go to $CONTENT_URL",
            null,
            "Go to ${contentUrl()}",
            "Go to {{event.id | contentUrl | default: 'null'}}"
        )]
        public async Task Should_format_content_url_from_event(string script)
        {
            var @event = new EnrichedContentEvent { AppId = appId, Id = contentId, SchemaId = schemaId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Go to content-url", result);
        }

        [Theory]
        [Expressions(
            "Go to $CONTENT_URL",
            null,
            "Go to ${contentUrl()}",
            "Go to {{event.id | contentUrl | default: 'null'}}"
        )]
        public async Task Should_return_null_when_content_url_when_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Go to null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_STATUS",
            "${EVENT_STATUS}",
            "${contentAction()}",
            "{{event.status}}"
        )]
        public async Task Should_format_content_status_when_found(string script)
        {
            var @event = new EnrichedContentEvent { Status = Status.Published };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Published", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_STATUS",
            "${EVENT_STATUS}",
            "${contentAction()}",
            "{{event.status | default: 'null'}}"
        )]
        public async Task Should_return_null_when_content_status_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_ACTION",
            "${EVENT_TYPE}",
            "${event.type}",
            "{{event.type}}"
        )]
        public async Task Should_format_content_actions_when_found(string script)
        {
            var @event = new EnrichedContentEvent { Type = EnrichedContentEventType.Created };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Created", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_STATUS",
            "${CONTENT_STATUS}",
            "${contentAction()}",
            null
        )]
        public async Task Should_return_null_when_content_action_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.country.zh-TW",
            "${CONTENT_DATA.country.zh-TW}",
            "${event.data.country['zh-TW']}",
            "{{event.data.country.zh-TW}}"
        )]
        public async Task Should_return_country_based_culture(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("country",
                            new ContentFieldData()
                                .AddValue("zh-TW", "Berlin"))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Berlin", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.country.iv",
            "${CONTENT_DATA.country.iv}",
            "${event.data.country.iv}",
            "{{event.data.country.iv | default: 'null'}}"
        )]
        public async Task Should_return_null_when_field_not_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.country.iv",
            "${CONTENT_DATA.country.iv}",
            "${event.data.country.iv}",
            "{{event.data.country.iv | default: 'null'}}"
        )]
        public async Task Should_return_null_when_partition_not_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.country.iv.10",
            "${CONTENT_DATA.country.iv.10}",
            "${event.data.country.iv[10]}",
            "{{event.data.country.iv[10] | default: 'null'}}"
        )]
        public async Task Should_return_null_when_array_item_not_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array()))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.country.iv.Location",
            "${CONTENT_DATA.country.iv.Location}",
            "${event.data.country.iv.Location}",
            "{{event.data.country.iv.Location | default: 'null'}}"
        )]
        public async Task Should_return_null_when_property_not_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Object().Add("name", "Berlin")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.city.iv",
            "${CONTENT_DATA.city.iv}",
            "${event.data.city.iv}",
            "{{event.data.city.iv}}"
        )]
        public async Task Should_return_plain_value_when_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddValue("iv", "Berlin"))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Berlin", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.city.iv.0",
            "${CONTENT_DATA.city.iv.0}",
            "${event.data.city.iv[0]}",
            "{{event.data.city.iv[0]}}"
        )]
        public async Task Should_return_plain_value_from_array_when_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array("Berlin")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Berlin", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.city.iv.name",
            "${CONTENT_DATA.city.iv.name}",
            "${event.data.city.iv.name}",
            "{{event.data.city.iv.name}}"
        )]
        public async Task Should_return_plain_value_from_object_when_found(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Object().Add("name", "Berlin")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Berlin", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.city.iv",
            "${CONTENT_DATA.city.iv}",
            "${JSON.stringify(event.data.city.iv)}",
            "{{event.data.city.iv}}"
        )]
        public async Task Should_return_json_string_when_object(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Object().Add("name", "Berlin")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("{\"name\":\"Berlin\"}", result);
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA.city.iv",
            "${CONTENT_DATA.city.iv}",
            "${JSON.stringify(event.data.city.iv)}",
            "{{event.data.city.iv}}"
        )]
        public async Task Should_return_json_string_when_array(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array(1, 2, 3)))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("[1,2,3]", result?.Replace(" ", string.Empty));
        }

        [Theory]
        [Expressions(
            "$CONTENT_DATA",
            "${CONTENT_DATA}",
            "${JSON.stringify(event.data)}",
            null
        )]
        public async Task Should_return_json_string_when_data(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Object().Add("name", "Berlin")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("{\"city\":{\"iv\":{\"name\":\"Berlin\"}}}", result);
        }

        [Theory]
        [Expressions(
            null,
            "From ${EVENT_ACTOR}",
            "From ${event.actor}",
            "From {{event.actor}}"
        )]
        public async Task Should_format_actor(string script)
        {
            var @event = new EnrichedContentEvent { Actor = new RefToken(RefTokenType.Client, "android") };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From client:android", result);
        }

        [Theory]
        [Expressions(
            null,
            "${ASSET_LASTMODIFIED | timestamp}",
            "${event.lastModified.getTime()}",
            "{{event.lastModified | timestamp}}"
        )]
        public async Task Should_transform_timestamp(string script)
        {
            var @event = new EnrichedAssetEvent { LastModified = Instant.FromUnixTimeSeconds(1590769584) };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("1590769584000", result);
        }

        [Theory]
        [Expressions(
            null,
            "${CONTENT_DATA.time.iv | timestamp}",
            null,
            "{{event.data.time.iv | timestamp}}")]
        public async Task Should_return_timestamp_when_string(string script)
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("time",
                            new ContentFieldData()
                                .AddValue(JsonValue.Create("2020-06-01T10:10:20Z")))
            };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("1591006220000", result);
        }

        [Theory]
        [Expressions(
            null,
            "${ASSET_LASTMODIFIED | timestamp_sec}",
            "${event.lastModified.getTime() / 1000}",
            "{{event.lastModified | timestamp_sec}}"
        )]
        public async Task Should_transform_timestamp_seconds(string script)
        {
            var @event = new EnrichedAssetEvent { LastModified = Instant.FromUnixTimeSeconds(1590769584) };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("1590769584", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Upper}",
            "${EVENT_USER.NAME | Upper}",
            "${event.user.name.toUpperCase()}",
            "{{event.user.name | upcase}}"
        )]
        public async Task Should_transform_upper(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("DONALD DUCK", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Lower}",
            "${EVENT_USER.NAME | Lower}",
            "${event.user.name.toLowerCase()}",
            "{{event.user.name | downcase}}"
        )]
        public async Task Should_transform_lower(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("donald duck", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Trim}",
            "${EVENT_USER.NAME | Trim}",
            "${event.user.name.trim()}",
            "{{event.user.name | trim}}"
        )]
        public async Task Should_transform_trimmed(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck  ") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Donald Duck", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Slugify}",
            "${EVENT_USER.NAME | Slugify}",
            "${slugify(event.user.name)}",
            "{{event.user.name | slugify}}"
        )]
        public async Task Should_transform_slugify(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("donald-duck", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Upper | Trim}",
            "${EVENT_USER.NAME | Upper | Trim}",
            "${event.user.name.toUpperCase().trim()}",
            "{{event.user.name | upcase | trim}}"
        )]
        public async Task Should_transform_chained(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck  ") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("DONALD DUCK", result);
        }

        [Theory]
        [Expressions(
            "${USER_NAME | Escape}",
            "${EVENT_USER.NAME | Escape}",
            null,
            "{{event.user.name | escape}}"
        )]
        public async Task Should_transform_json_escape(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald\"Duck") });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Donald\\\"Duck", result);
        }
    }
}
