// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Infrastructure.Validation;
using Squidex.Log;
using Xunit;

namespace Squidex.Infrastructure.Orleans
{
    public class LoggingFilterTests
    {
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IIncomingGrainCallContext context = A.Fake<IIncomingGrainCallContext>();
        private readonly LoggingFilter sut;

        public LoggingFilterTests()
        {
            sut = new LoggingFilter(log);
        }

        [Fact]
        public async Task Should_not_log_if_no_exception_happened()
        {
            await sut.Invoke(context);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_log_domain_exceptions()
        {
            A.CallTo(() => context.Invoke())
                .Throws(new ValidationException("Failed"));

            await Assert.ThrowsAsync<ValidationException>(() => sut.Invoke(context));

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception_and_forward_it()
        {
            A.CallTo(() => context.Invoke())
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Invoke(context));

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }
    }
}
