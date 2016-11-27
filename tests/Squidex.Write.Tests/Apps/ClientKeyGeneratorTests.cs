// ==========================================================================
//  ClientKeyGeneratorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Write.Apps
{
    public class ClientKeyGeneratorTests
    {
        private readonly ClientKeyGenerator sut = new ClientKeyGenerator();

        [Fact]
        public void Should_create_very_long_client_key()
        {
            var key = sut.GenerateKey();

            Assert.Equal(44, key.Length);
        }
    }
}
