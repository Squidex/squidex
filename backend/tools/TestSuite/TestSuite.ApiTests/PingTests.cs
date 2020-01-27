﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class PingTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public PingTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_ping_service()
        {
            await _.Ping.GetPingAsync();
        }

        [Fact]
        public async Task Should_ping_app()
        {
            await _.Ping.GetAppPingAsync(_.AppName);
        }

        [Fact]
        public async Task Should_get_info()
        {
            var infos = await _.Ping.GetInfoAsync();

            Assert.NotNull(infos);
        }
    }
}
