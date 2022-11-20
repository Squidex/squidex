// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Email;

[Trait("Category", "Dependencies")]
public class SmtpEmailSenderTests
{
    [Fact]
    public async Task Should_handle_timeout_properly()
    {
        var options = TestConfig.Configuration.GetSection("email:smtp").Get<SmtpOptions>()!;

        var recipient = TestConfig.Configuration["email:smtp:recipient"]!;

        var testSubject = TestConfig.Configuration["email:smtp:testSubject"]!;
        var testBody = TestConfig.Configuration["email:smtp:testBody"]!;

        var sut = new SmtpEmailSender(Options.Create(options));

        using (var cts = new CancellationTokenSource(5000))
        {
            await sut.SendAsync(recipient, testSubject, testBody, cts.Token);
        }
    }
}
