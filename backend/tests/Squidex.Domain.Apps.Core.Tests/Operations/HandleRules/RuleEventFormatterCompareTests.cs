// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
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

#pragma warning disable CA2012 // Use ValueTasks correctly

namespace Squidex.Domain.Apps.Core.Operations.HandleRules;

public class RuleEventFormatterCompareTests
{
    private readonly IUser user = UserMocks.User("user123", "me@email.com", "me");
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly DomainId assetId = DomainId.NewGuid();
    private readonly RuleEventFormatter sut;

    private sealed class FakeContentResolver : IRuleEventFormatter
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

        return new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
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

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal($"Name my-app has id {appId.Id}", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, SchemaId = schemaId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal($"Name my-schema has id {schemaId.Id}", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, Timestamp = now };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal($"DateTime: {now:yyyy-MM-dd-hh-mm-ss}", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, Timestamp = now };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal($"Date: {now:yyyy-MM-dd}", actual);
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
        var @event = new EnrichedCommentEvent { AppId = appId, MentionedUser = user };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("From me (me@email.com, user123)", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("From me (me@email.com, user123)", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("From null (null, null)", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = new ClientUser(RefToken.Client("android")) };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("From android (client:android, android)", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, Version = 13 };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Version: 13", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, FileName = "my-file.png" };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("File: my-file.png", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, AssetType = AssetType.Audio };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Type: Audio", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, Id = assetId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at asset-content-url", actual);
    }

    [Theory]
    [Expressions(
        "Download at $ASSET_CONTENT_URL",
        null,
        "Download at ${assetContentUrl()}",
        "Download at {{event.id | assetContentUrl | default: 'null'}}"
    )]
    [InlineData("Liquid(Download at {{event | assetContentUrl | default: 'null'}})")]
    public async Task Should_return_null_if_asset_content_url_not_found(string script)
    {
        var @event = new EnrichedContentEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at null", actual);
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

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at asset-content-url", actual);
    }

    [Theory]
    [Expressions(
        "Download at $ASSET_CONTENT_APP_URL",
        null,
        "Download at ${assetContentAppUrl()}",
        "Download at {{event.id | assetContentAppUrl | default: 'null'}}"
    )]
    [InlineData("Liquid(Download at {{event | assetContentAppUrl | default: 'null'}})")]
    public async Task Should_return_null_if_asset_content_app_url_not_found(string script)
    {
        var @event = new EnrichedContentEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at null", actual);
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

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at asset-content-slug-url", actual);
    }

    [Theory]
    [Expressions(
        "Download at $ASSET_CONTENT_SLUG_URL",
        null,
        "Download at ${assetContentSlugUrl()}",
        "Download at {{event.id | assetContentSlugUrl | default: 'null'}}"
    )]
    [InlineData("Liquid(Download at {{event | assetContentSlugUrl | default: 'null'}})")]
    public async Task Should_return_null_if_asset_content_slug_url_not_found(string script)
    {
        var @event = new EnrichedContentEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Download at null", actual);
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

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Go to content-url", actual);
    }

    [Theory]
    [Expressions(
        "Go to $CONTENT_URL",
        null,
        "Go to ${contentUrl()}",
        "Go to {{event.id | contentUrl | default: 'null'}}"
    )]
    public async Task Should_return_null_if_content_url_if_not_found(string script)
    {
        var @event = new EnrichedAssetEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Go to null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_STATUS",
        "${EVENT_STATUS}",
        "${contentAction()}",
        "{{event.status}}"
    )]
    public async Task Should_format_content_status_if_found(string script)
    {
        var @event = new EnrichedContentEvent { AppId = appId, Status = Status.Published };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Published", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_STATUS",
        "${EVENT_STATUS}",
        "${contentAction()}",
        "{{event.status | default: 'null'}}"
    )]
    public async Task Should_return_null_if_content_status_not_found(string script)
    {
        var @event = new EnrichedAssetEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_ACTION",
        "${EVENT_TYPE}",
        "${event.type}",
        "{{event.type}}"
    )]
    public async Task Should_format_content_actions_if_found(string script)
    {
        var @event = new EnrichedContentEvent { AppId = appId, Type = EnrichedContentEventType.Created };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Created", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_STATUS",
        "${CONTENT_STATUS}",
        "${contentAction()}",
        null
    )]
    public async Task Should_return_null_if_content_action_not_found(string script)
    {
        var @event = new EnrichedAssetEvent { AppId = appId };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
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
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("country",
                        new ContentFieldData()
                            .AddLocalized("zh-TW", "Berlin"))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Berlin", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.country.iv",
        "${CONTENT_DATA.country.iv}",
        "${event.data.country.iv}",
        "{{event.data.country.iv | default: 'null'}}"
    )]
    public async Task Should_return_null_if_field_not_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant("Berlin"))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.country.iv",
        "${CONTENT_DATA.country.iv}",
        "${event.data.country.iv}",
        "{{event.data.country.iv | default: 'null'}}"
    )]
    public async Task Should_return_null_if_partition_not_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant("Berlin"))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.country.iv.10",
        "${CONTENT_DATA.country.iv.10}",
        "${event.data.country.iv[10]}",
        "{{event.data.country.iv[10] | default: 'null'}}"
    )]
    public async Task Should_return_null_if_array_item_not_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(new JsonArray()))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.country.iv.Location",
        "${CONTENT_DATA.country.iv.Location}",
        "${event.data.country.iv.Location}",
        "{{event.data.country.iv.Location | default: 'null'}}"
    )]
    public async Task Should_return_null_if_property_not_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(new JsonObject().Add("name", "Berlin")))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("null", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.city.iv",
        "${CONTENT_DATA.city.iv}",
        "${event.data.city.iv}",
        "{{event.data.city.iv}}"
    )]
    public async Task Should_return_plain_value_if_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant("Berlin"))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Berlin", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.city.iv.0",
        "${CONTENT_DATA.city.iv.0}",
        "${event.data.city.iv[0]}",
        "{{event.data.city.iv[0]}}"
    )]
    public async Task Should_return_plain_value_from_array_if_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array("Berlin")))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Berlin", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.city.iv.name",
        "${CONTENT_DATA.city.iv.name}",
        "${event.data.city.iv.name}",
        "{{event.data.city.iv.name}}"
    )]
    public async Task Should_return_plain_value_from_object_if_found(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(new JsonObject().Add("name", "Berlin")))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Berlin", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.city.iv",
        "${CONTENT_DATA.city.iv}",
        "${JSON.stringify(event.data.city.iv)}",
        "{{event.data.city.iv}}"
    )]
    public async Task Should_return_json_string_if_object(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(new JsonObject().Add("name", "Berlin")))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("{\"name\":\"Berlin\"}", actual);
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA.city.iv",
        "${CONTENT_DATA.city.iv}",
        "${JSON.stringify(event.data.city.iv)}",
        "{{event.data.city.iv}}"
    )]
    public async Task Should_return_json_string_if_array(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(1, 2, 3)))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("[1,2,3]", actual?.Replace(" ", string.Empty, StringComparison.Ordinal));
    }

    [Theory]
    [Expressions(
        "$CONTENT_DATA",
        "${CONTENT_DATA}",
        "${JSON.stringify(event.data)}",
        null
    )]
    public async Task Should_return_json_string_if_data(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("city",
                        new ContentFieldData()
                            .AddInvariant(new JsonObject().Add("name", "Berlin")))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("{\"city\":{\"iv\":{\"name\":\"Berlin\"}}}", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, Actor = RefToken.Client("android") };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("From client:android", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, LastModified = Instant.FromUnixTimeSeconds(1590769584) };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("1590769584000", actual);
    }

    [Theory]
    [Expressions(
        null,
        "${CONTENT_DATA.time.iv | timestamp}",
        null,
        "{{event.data.time.iv | timestamp}}")]
    public async Task Should_return_timestamp_if_string(string script)
    {
        var @event = new EnrichedContentEvent
        {
            AppId = appId,
            Data =
                new ContentData()
                    .AddField("time",
                        new ContentFieldData()
                            .AddInvariant("2020-06-01T10:10:20Z"))
        };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("1591006220000", actual);
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
        var @event = new EnrichedAssetEvent { AppId = appId, LastModified = Instant.FromUnixTimeSeconds(1590769584) };

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("1590769584", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("DONALD DUCK", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("donald duck", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck  ") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Donald Duck", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("donald-duck", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald Duck  ") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("DONALD DUCK", actual);
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
        var @event = new EnrichedContentEvent { AppId = appId, User = user };

        A.CallTo(() => user.Claims)
            .Returns(new List<Claim> { new Claim(SquidexClaimTypes.DisplayName, "Donald\"Duck") });

        var actual = await sut.FormatAsync(script, @event);

        Assert.Equal("Donald\\\"Duck", actual);
    }
}
