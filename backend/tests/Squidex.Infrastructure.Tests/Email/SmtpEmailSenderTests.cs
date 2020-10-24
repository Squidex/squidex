// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.Email
{
    [Trait("Category", "Dependencies")]
    public class SmtpEmailSenderTests
    {
        [Fact]
        public async Task Should_handle_timeout_properly()
        {
            var sut = new SmtpEmailSender(Options.Create(new SmtpOptions
            {
                Sender = "sebastian@squidex.io",
                Server = "invalid",
                Timeout = 1000
            }));

            var timer = Task.Delay(5000);

            var result = await Task.WhenAny(timer, sut.SendAsync("hello@squidex.io", "TEST", "TEST"));

            Assert.NotSame(timer, result);
        }
    }
}
