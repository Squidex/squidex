// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class LoggingFilterTests
    {
        private readonly ILoggerFactory logFactory = A.Fake<ILoggerFactory>();
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();
        private readonly LoggingFilter sut;

        public LoggingFilterTests()
        {
            sut = new LoggingFilter(logFactory);
        }

        [Fact]
        public async Task Should_not_log_if_no_exception_happened()
        {
            await sut.Invoke(context);

            A.CallTo(() => logFactory.CreateLogger(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_log_domain_exceptions()
        {
            A.CallTo(() => context.Invoke())
                .Throws(new ValidationException("Failed"));

            await Assert.ThrowsAsync<ValidationException>(() => sut.Invoke(context));

            A.CallTo(() => logFactory.CreateLogger(A<string>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception_and_forward_it()
        {
            var log = A.Fake<ILogger>();

            var grain = A.Fake<IAddressable>();

            A.CallTo(() => context.Invoke())
                .Throws(new InvalidOperationException());

            A.CallTo(() => context.Grain)
                .Returns(grain);

            A.CallTo(() => logFactory.CreateLogger(A<string>._))
                .Returns(log);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Invoke(context));

            A.CallTo(log).Where(x => x.Method.Name == "Log")
                .MustHaveHappened();
        }
    }
}
