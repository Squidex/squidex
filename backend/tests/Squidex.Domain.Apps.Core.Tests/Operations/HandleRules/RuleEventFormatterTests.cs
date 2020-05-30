// ==========================================================================
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
using Squidex.Domain.Apps.Core.HandleRules.Scripting;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.HandleRules
{
    public class RuleEventFormatterTests
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
                if (path[0] == "data" && value is JsonArray _)
                {
                    return (true, GetValueAsync());
                }

                return default;
            }

            private async ValueTask<string?> GetValueAsync()
            {
                await Task.Delay(5);

                return "Reference";
            }
        }

        public RuleEventFormatterTests()
        {
            A.CallTo(() => urlGenerator.ContentUI(appId, schemaId, contentId))
                .Returns("content-url");

            A.CallTo(() => urlGenerator.AssetContent(assetId))
                .Returns("asset-content-url");

            A.CallTo(() => user.Id)
                .Returns("user123");

            A.CallTo(() => user.Email)
                .Returns("me@email.com");

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "me") });

            var extensions = new IScriptExtension[]
            {
                new DateTimeScriptExtension(),
                new EventScriptExtension(urlGenerator),
                new StringScriptExtension()
            };

            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            var formatters = new IRuleEventFormatter[]
            {
                new PredefinedPatternsFormatter(urlGenerator),
                new FakeContentResolver()
            };

            sut = new RuleEventFormatter(TestUtils.DefaultSerializer, formatters, new JintScriptEngine(cache, extensions));
        }

        [Fact]
        public void Should_serialize_object_to_json()
        {
            var result = sut.ToPayload(new { Value = 1 });

            Assert.NotNull(result);
        }

        [Fact]
        public void Should_create_payload()
        {
            var @event = new EnrichedContentEvent { AppId = appId };

            var result = sut.ToPayload(@event);

            Assert.NotNull(result);
        }

        [Fact]
        public void Should_create_envelope_data_from_event()
        {
            var @event = new EnrichedContentEvent { AppId = appId, Name = "MyEventName" };

            var result = sut.ToEnvelope(@event);

            Assert.Contains("MyEventName", result);
        }

        [Theory]
        [InlineData("Name $APP_NAME has id $APP_ID")]
        [InlineData("Name ${EVENT_APPID.NAME} has id ${EVENT_APPID.ID}")]
        [InlineData("Script(`Name ${event.appId.name} has id ${event.appId.id}`)")]
        public async Task Should_format_app_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { AppId = appId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Name my-app has id {appId.Id}", result);
        }

        [Theory]
        [InlineData("Name $SCHEMA_NAME has id $SCHEMA_ID")]
        [InlineData("Script(`Name ${event.schemaId.name} has id ${event.schemaId.id}`)")]
        public async Task Should_format_schema_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { SchemaId = schemaId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Name my-schema has id {schemaId.Id}", result);
        }

        [Theory]
        [InlineData("Full: $TIMESTAMP_DATETIME")]
        [InlineData("Script(`Full: ${formatDate(event.timestamp, 'yyyy-MM-dd-hh-mm-ss')}`)")]
        public async Task Should_format_timestamp_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { Timestamp = now };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Full: {now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Theory]
        [InlineData("Date: $TIMESTAMP_DATE")]
        [InlineData("Script(`Date: ${formatDate(event.timestamp, 'yyyy-MM-dd')}`)")]
        public async Task Should_format_timestamp_date_information_from_event(string script)
        {
            var @event = new EnrichedContentEvent { Timestamp = now };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal($"Date: {now:yyyy-MM-dd}", result);
        }

        [Theory]
        [InlineData("From $MENTIONED_NAME ($MENTIONED_EMAIL, $MENTIONED_ID)")]
        [InlineData("From ${COMMENT_MENTIONEDUSER.NAME} (${COMMENT_MENTIONEDUSER.EMAIL}, ${COMMENT_MENTIONEDUSER.ID})")]
        [InlineData("Script(`From ${event.mentionedUser.name} (${event.mentionedUser.email}, ${event.mentionedUser.id})`)")]
        public async Task Should_format_email_and_display_name_from_mentioned_user(string script)
        {
            var @event = new EnrichedCommentEvent { MentionedUser = user };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From me (me@email.com, user123)", result);
        }

        [Theory]
        [InlineData("From $USER_NAME ($USER_EMAIL, $USER_ID)")]
        [InlineData("Script(`From ${event.user.name} (${event.user.email}, ${event.user.id})`)")]
        public async Task Should_format_email_and_display_name_from_user(string script)
        {
            var @event = new EnrichedContentEvent { User = user };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From me (me@email.com, user123)", result);
        }

        [Theory]
        [InlineData("From $USER_NAME ($USER_EMAIL, $USER_ID)")]
        [InlineData("Script(`From ${event.user.name} (${event.user.email}, ${event.user.id})`)")]
        public async Task Should_return_null_if_user_is_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From null (null, null)", result);
        }

        [Theory]
        [InlineData("From $USER_NAME ($USER_EMAIL, $USER_ID)")]
        [InlineData("Script(`From ${event.user.name} (${event.user.email}, ${event.user.id})`)")]
        public async Task Should_format_email_and_display_name_from_client(string script)
        {
            var @event = new EnrichedContentEvent { User = new ClientUser(new RefToken(RefTokenType.Client, "android")) };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From client:android (client:android, android)", result);
        }

        [Theory]
        [InlineData("Version: $ASSET_VERSION")]
        [InlineData("Script(`Version: ${event.version}`)")]
        public async Task Should_format_base_property(string script)
        {
            var @event = new EnrichedAssetEvent { Version = 13 };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Version: 13", result);
        }

        [Theory]
        [InlineData("File: $ASSET_FILENAME")]
        [InlineData("Script(`File: ${event.fileName}`)")]
        public async Task Should_format_asset_file_name_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { FileName = "my-file.png" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("File: my-file.png", result);
        }

        [Theory]
        [InlineData("Type: $ASSET_ASSETTYPE")]
        [InlineData("Script(`Type: ${event.assetType}`)")]
        public async Task Should_format_asset_asset_type_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { AssetType = AssetType.Audio };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Type: Audio", result);
        }

        [Theory]
        [InlineData("Download at $ASSET_CONTENT_URL")]
        [InlineData("Script(`Download at ${assetContentUrl()}`)")]
        public async Task Should_format_asset_content_url_from_event(string script)
        {
            var @event = new EnrichedAssetEvent { Id = assetId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at asset-content-url", result);
        }

        [Theory]
        [InlineData("Download at $ASSET_CONTENT_URL")]
        [InlineData("Script(`Download at ${assetContentUrl()}`)")]
        public async Task Should_return_null_when_asset_content_url_not_found(string script)
        {
            var @event = new EnrichedContentEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Download at null", result);
        }

        [Theory]
        [InlineData("Go to $CONTENT_URL")]
        [InlineData("Script(`Go to ${contentUrl()}`)")]
        public async Task Should_format_content_url_from_event(string script)
        {
            var @event = new EnrichedContentEvent { AppId = appId, Id = contentId, SchemaId = schemaId };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Go to content-url", result);
        }

        [Theory]
        [InlineData("Go to $CONTENT_URL")]
        [InlineData("Script(`Go to ${contentUrl()}`)")]
        public async Task Should_return_null_when_content_url_when_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Go to null", result);
        }

        [Theory]
        [InlineData("$CONTENT_STATUS")]
        [InlineData("Script(contentAction())")]
        [InlineData("Script(`${event.status}`)")]
        public async Task Should_format_content_status_when_found(string script)
        {
            var @event = new EnrichedContentEvent { Status = Status.Published };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Published", result);
        }

        [Theory]
        [InlineData("$CONTENT_ACTION")]
        [InlineData("Script(contentAction())")]
        public async Task Should_return_null_when_content_status_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [InlineData("$CONTENT_ACTION")]
        [InlineData("Script(`${event.type}`)")]
        public async Task Should_format_content_actions_when_found(string script)
        {
            var @event = new EnrichedContentEvent { Type = EnrichedContentEventType.Created };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("Created", result);
        }

        [Theory]
        [InlineData("$CONTENT_ACTION")]
        [InlineData("Script(contentAction())")]
        public async Task Should_return_null_when_content_action_not_found(string script)
        {
            var @event = new EnrichedAssetEvent();

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("null", result);
        }

        [Theory]
        [InlineData("$CONTENT_DATA.country.iv")]
        [InlineData("Script(`${event.data.country.iv}`)")]
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
        [InlineData("$CONTENT_DATA.city.de")]
        [InlineData("Script(`${event.data.country.iv}`)")]
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
        [InlineData("$CONTENT_DATA.city.iv.10")]
        [InlineData("Script(`${event.data.country.de[10]}`)")]
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
        [InlineData("$CONTENT_DATA.city.de.Name")]
        [InlineData("Script(`${event.data.city.de.Location}`)")]
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
        [InlineData("$CONTENT_DATA.city.iv")]
        [InlineData("Script(`${event.data.city.iv}`)")]
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
        [InlineData("$CONTENT_DATA.city.iv.0")]
        [InlineData("Script(`${event.data.city.iv[0]}`)")]
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
        [InlineData("$CONTENT_DATA.city.iv.name")]
        [InlineData("Script(`${event.data.city.iv.name}`)")]
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
        [InlineData("$CONTENT_DATA.city.iv")]
        [InlineData("Script(`${JSON.stringify(event.data.city.iv)}`)")]
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

        [Fact]
        public async Task Should_resolve_reference()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("city",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array()))
            };

            var result = await sut.FormatAsync("${CONTENT_DATA.city.iv.data.name}", @event);

            Assert.Equal("Reference", result);
        }

        [Theory]
        [InlineData("Script(`From ${event.actor}`)")]
        public async Task Should_format_actor(string script)
        {
            var @event = new EnrichedContentEvent { Actor = new RefToken(RefTokenType.Client, "android") };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("From client:android", result);
        }

        [Theory]
        [InlineData("${EVENT_INVALID ? file}", "file")]
        public async Task Should_provide_fallback_if_path_is_invalid(string script, string expect)
        {
            var @event = new EnrichedAssetEvent { FileName = null! };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("${ASSET_FILENAME ? file}", "file")]
        public async Task Should_provide_fallback_if_value_is_null(string script, string expect)
        {
            var @event = new EnrichedAssetEvent { FileName = null! };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("Found in ${ASSET_FILENAME | Upper}.docx", "Found in DONALD DUCK.docx")]
        [InlineData("Found in ${ASSET_FILENAME| Upper  }.docx", "Found in DONALD DUCK.docx")]
        [InlineData("Found in ${ASSET_FILENAME|Upper }.docx", "Found in DONALD DUCK.docx")]
        public async Task Should_transform_replacements_and_igore_whitepsaces(string script, string expect)
        {
            var @event = new EnrichedAssetEvent { FileName = "Donald Duck" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("Found in ${ASSET_FILENAME | Escape | Upper}.docx", "Found in DONALD\\\"DUCK.docx", "Donald\"Duck")]
        [InlineData("Found in ${ASSET_FILENAME | Escape}.docx", "Found in Donald\\\"Duck.docx", "Donald\"Duck")]
        [InlineData("Found in ${ASSET_FILENAME | Upper}.docx", "Found in DONALD DUCK.docx", "Donald Duck")]
        [InlineData("Found in ${ASSET_FILENAME | Lower}.docx", "Found in donald duck.docx", "Donald Duck")]
        [InlineData("Found in ${ASSET_FILENAME | Slugify}.docx", "Found in donald-duck.docx", "Donald Duck")]
        [InlineData("Found in ${ASSET_FILENAME | Trim}.docx", "Found in Donald Duck.docx", "Donald Duck ")]
        public async Task Should_transform_replacements(string script, string expect, string name)
        {
            var @event = new EnrichedAssetEvent { FileName = name };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("From ${USER_NAME | Escape | Upper}", "From DONALD\\\"DUCK", "Donald\"Duck")]
        [InlineData("From ${USER_NAME | Escape}", "From Donald\\\"Duck", "Donald\"Duck")]
        [InlineData("From ${USER_NAME | Upper}", "From DONALD DUCK", "Donald Duck")]
        [InlineData("From ${USER_NAME | Lower}", "From donald duck", "Donald Duck")]
        [InlineData("From ${USER_NAME | Slugify}", "From donald-duck", "Donald Duck")]
        [InlineData("From ${USER_NAME | Trim}", "From Donald Duck", "Donald Duck ")]
        public async Task Should_transform_replacements_with_simple_pattern(string script, string expect, string name)
        {
            var @event = new EnrichedContentEvent { User = user };

            A.CallTo(() => user.Claims)
                .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, name) });

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("{'Key':'${ASSET_FILENAME | Upper}'}", "{'Key':'DONALD DUCK'}")]
        [InlineData("{'Key':'${ASSET_FILENAME}'}", "{'Key':'Donald Duck'}")]
        public async Task Should_transform_json_examples(string script, string expect)
        {
            var @event = new EnrichedAssetEvent { FileName = "Donald Duck" };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Theory]
        [InlineData("${ASSET_LASTMODIFIED | timestamp_seconds}", "1590769584")]
        [InlineData("${ASSET_LASTMODIFIED | timestamp_ms}", "1590769584000")]
        public async Task Should_transform_timestamp(string script, string expect)
        {
            var @event = new EnrichedAssetEvent { LastModified = Instant.FromUnixTimeSeconds(1590769584) };

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal(expect, result);
        }

        [Fact]
        public async Task Should_format_json()
        {
            var @event = new EnrichedContentEvent { Actor = new RefToken(RefTokenType.Client, "android") };

            var result = await sut.FormatAsync("Script(JSON.stringify({ actor: event.actor.toString() }))", @event);

            Assert.Equal("{\"actor\":\"client:android\"}", result);
        }

        [Fact]
        public async Task Should_format_json_with_special_characters()
        {
            var @event = new EnrichedContentEvent { Actor = new RefToken(RefTokenType.Client, "mobile\"android") };

            var result = await sut.FormatAsync("Script(JSON.stringify({ actor: event.actor.toString() }))", @event);

            Assert.Equal("{\"actor\":\"client:mobile\\\"android\"}", result);
        }

        [Fact]
        public async Task Should_evaluate_script_if_starting_with_whitespace()
        {
            var @event = new EnrichedContentEvent { Type = EnrichedContentEventType.Created };

            var result = await sut.FormatAsync(" Script(`${event.type}`)", @event);

            Assert.Equal("Created", result);
        }

        [Fact]
        public async Task Should_evaluate_script_if_ends_with_whitespace()
        {
            var @event = new EnrichedContentEvent { Type = EnrichedContentEventType.Created };

            var result = await sut.FormatAsync("Script(`${event.type}`) ", @event);

            Assert.Equal("Created", result);
        }
    }
}
