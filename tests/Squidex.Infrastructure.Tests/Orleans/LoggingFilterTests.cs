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
using Squidex.Infrastructure.Log;
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

            A.CallTo(() => log.Log(A<SemanticLogLevel>.Ignored, A<None>.Ignored, A<Action<None, IObjectWriter>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_log_exception_and_forward_it()
        {
            A.CallTo(() => context.Invoke())
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Invoke(context));

            A.CallTo(() => log.Log(A<SemanticLogLevel>.Ignored, A<None>.Ignored, A<Action<None, IObjectWriter>>.Ignored))
                .MustHaveHappened();
        }
    }
}
