// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class GravatarHelperTests
    {
        [Theory]
        [InlineData("me@email.com ")]
        [InlineData("me@email.com")]
        [InlineData("ME@email.com")]
        public void Should_generate_picture_url(string email)
        {
            var url = GravatarHelper.CreatePictureUrl(email);

            Assert.Equal("https://www.gravatar.com/avatar/8f9dc04e6abdcc9fea53e81945c7294b", url);
        }

        [Theory]
        [InlineData("me@email.com ")]
        [InlineData("me@email.com")]
        [InlineData("ME@email.com")]
        public void Should_generate_profile_url(string email)
        {
            var url = GravatarHelper.CreateProfileUrl(email);

            Assert.Equal("https://www.gravatar.com/8f9dc04e6abdcc9fea53e81945c7294b", url);
        }
    }
}