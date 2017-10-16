// ==========================================================================
//  GravatarHelperTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure
{
    public class GravatarHelperTests
    {
        [Theory]
        [InlineData("MyEmailAddress@example.com ")]
        [InlineData("MyEmailAddress@example.com")]
        [InlineData("myemailaddress@example.com")]
        public void Should_generate_picture_url(string email)
        {
            var url = GravatarHelper.CreatePictureUrl(email);

            Assert.Equal("https://www.gravatar.com/avatar/0bc83cb571cd1c50ba6f3e8a78ef1346", url);
        }

        [Theory]
        [InlineData("MyEmailAddress@example.com ")]
        [InlineData("MyEmailAddress@example.com")]
        [InlineData("myemailaddress@example.com")]
        public void Should_generate_profile_url(string email)
        {
            var url = GravatarHelper.CreateProfileUrl(email);

            Assert.Equal("https://www.gravatar.com/0bc83cb571cd1c50ba6f3e8a78ef1346", url);
        }
    }
}