// ==========================================================================
//  RefTokenTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure
{
    public class RefTokenTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(":")]
        [InlineData("user")]
        public void Should_throw_exception_if_parsing_invalid_input(string input)
        {
            Assert.Throws<ArgumentException>(() => RefToken.Parse(input));
        }

        [Fact]
        public void Should_instantiate_token()
        {
            var token = new RefToken("client", "client1");

            Assert.Equal("client", token.Type);
            Assert.Equal("client1", token.Identifier);
        }

        [Fact]
        public void Should_instantiate_token_and_lower_type()
        {
            var token = new RefToken("Client", "client1");

            Assert.Equal("client", token.Type);
            Assert.Equal("client1", token.Identifier);
        }

        [Fact]
        public void Should_parse_user_token_from_string()
        {
            var token = RefToken.Parse("client:client1");

            Assert.Equal("client", token.Type);
            Assert.Equal("client1", token.Identifier);
        }

        [Fact]
        public void Should_parse_user_token_with_colon_in_identifier()
        {
            var token = RefToken.Parse("client:client1:app");

            Assert.Equal("client", token.Type);
            Assert.Equal("client1:app", token.Identifier);
        }

        [Fact]
        public void Should_convert_user_token_to_string()
        {
            var token = RefToken.Parse("client:client1");

            Assert.Equal("client:client1", token.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var token1a = RefToken.Parse("client:client1");
            var token1b = RefToken.Parse("client:client1");
            var token2a = RefToken.Parse("client:client2");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            object token1a = RefToken.Parse("client:client1");
            object token1b = RefToken.Parse("client:client1");
            object token2a = RefToken.Parse("client:client2");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var token1a = RefToken.Parse("client:client1");
            var token1b = RefToken.Parse("client:client1");
            var token2a = RefToken.Parse("client:client2");

            Assert.Equal(token1a.GetHashCode(), token1b.GetHashCode());

            Assert.NotEqual(token1a.GetHashCode(), token2a.GetHashCode());
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_token()
        {
            JsonHelper.SerializeAndDeserialize<RefToken>(null, new RefTokenConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_token()
        {
            RefToken.Parse("client:client1").SerializeAndDeserialize(new RefTokenConverter());
        }
    }
}
