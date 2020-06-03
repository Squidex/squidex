﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.Templates
{
    public class FluidTemplateEngineTests
    {
        private readonly FluidTemplateEngine sut;

        public FluidTemplateEngineTests()
        {
            var extensions = new IFluidExtension[]
            {
                new DateTimeFluidExtensions()
            };

            sut = new FluidTemplateEngine(extensions);
        }

        [Theory]
        [InlineData("{{ e.user }}", "subject:me")]
        [InlineData("{{ e.user.type }}", "subject")]
        [InlineData("{{ e.user.identifier }}", "me")]
        public async Task Should_render_ref_token(string template, string expected)
        {
            var value = new
            {
                User = new RefToken(RefTokenType.Subject, "me")
            };

            var (result, errors) = await RenderAync(template, value);

            Assert.Empty(errors);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{{ e.id }}", "42,my-app")]
        [InlineData("{{ e.id.name}}", "my-app")]
        [InlineData("{{ e.id.id }}", "42")]
        public async Task Should_render_named_id(string template, string expected)
        {
            var value = new
            {
                Id = NamedId.Of("42", "my-app")
            };

            var (result, errors) = await RenderAync(template, value);

            Assert.Empty(errors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_format_date()
        {
            var now = DateTime.UtcNow;

            var value = new
            {
                Timestamp = now
            };

            var template = "{{ e.timestamp | formatDate: 'yyyy-MM-dd-hh-mm-ss' }}";

            var (result, errors) = await RenderAync(template, value);

            Assert.Empty(errors);
            Assert.Equal($"{now:yyyy-MM-dd-hh-mm-ss}", result);
        }

        [Fact]
        public async Task Should_format_content_data()
        {
            var template = "{{ e.data.value.en }}";

            var value = new
            {
                Data =
                    new NamedContentData()
                        .AddField("value",
                            new ContentFieldData()
                                .AddValue("en", "Hello"))
            };

            var (result, errors) = await RenderAync(template, value);

            Assert.Empty(errors);
            Assert.Equal("Hello", result);
        }

        private Task<(string? Result, IEnumerable<string> Errors)> RenderAync(string template, object value)
        {
            return sut.RenderAsync(template, new TemplateVars { ["e"] = value });
        }
    }
}
