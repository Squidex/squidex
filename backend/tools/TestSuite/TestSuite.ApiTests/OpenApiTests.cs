// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests
{
    public class OpenApiTests : IClassFixture<ClientManagerFixture>
    {
        public ClientManagerFixture _ { get; }

        public OpenApiTests(ClientManagerFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_provide_spec()
        {
            var url = $"{_.ClientManager.Options.Url}/api/swagger/v1/swagger.json";

            var document = await OpenApiDocument.FromUrlAsync(url);

            Assert.NotNull(document);
        }
    }
}
