// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

            Assert.True(token.IsClient);
        }

        [Fact]
        public void Should_instantiate_subject_token()
        {
            var token = new RefToken("subject", "client1");

            Assert.True(token.IsSubject);
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
        public void Should_serialize_and_deserialize_null_token()
        {
            RefToken? value = null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_token()
        {
            var value = RefToken.Parse("client:client1");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }
    }
}
