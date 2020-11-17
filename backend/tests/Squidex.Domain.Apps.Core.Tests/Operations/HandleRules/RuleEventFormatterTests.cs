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
    public class RuleEventFormatterTests
    {
        private readonly IUser user = A.Fake<IUser>();
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
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

        public RuleEventFormatterTests()
        {
            A.CallTo(() => urlGenerator.ContentUI(appId, schemaId, contentId))
                .Returns("content-url");

            A.CallTo(() => urlGenerator.AssetContent(appId, assetId.ToString()))
                .Returns("asset-content-url");

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

        private static FluidTemplateEngine BuildTemplateEngine()
        {
            var extensions = new IFluidExtension[]
            {
                new DateTimeFluidExtension(),
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

        [Fact]
        public async Task Should_return_json_string_when_array()
        {
            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("categories",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array("ref1", "ref2", "ref3")))
            };

            var script = @"
                Script(JSON.stringify(
                {
                    'categories': event.data.categories.iv
                }))";

            var result = await sut.FormatAsync(script, @event);

            Assert.Equal("{'categories':['ref1','ref2','ref3']}", result?.Replace(" ", string.Empty).Replace("\"", "'"));
        }
    }
}
