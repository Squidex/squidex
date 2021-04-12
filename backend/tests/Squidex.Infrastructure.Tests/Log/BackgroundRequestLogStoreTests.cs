// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Log;
using Xunit;

namespace Squidex.Infrastructure.Log
{
    public class BackgroundRequestLogStoreTests
    {
        private readonly IRequestLogRepository requestLogRepository = A.Fake<IRequestLogRepository>();
        private readonly RequestLogStoreOptions options = new RequestLogStoreOptions();
        private readonly BackgroundRequestLogStore sut;

        public BackgroundRequestLogStoreTests()
        {
            options.StoreEnabled = true;

            sut = new BackgroundRequestLogStore(Options.Create(options), requestLogRepository, A.Fake<ISemanticLog>());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_provide_disabled_from_options(bool enabled)
        {
            options.StoreEnabled = enabled;

            Assert.Equal(enabled, sut.IsEnabled);
        }

        [Fact]
        public async Task Should_not_if_disabled()
        {
            options.StoreEnabled = false;

            for (var i = 0; i < 2500; i++)
            {
                await sut.LogAsync(new Request { Key = i.ToString() });
            }

            sut.Next();
            sut.Dispose();

            A.CallTo(() => requestLogRepository.InsertManyAsync(A<IEnumerable<Request>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_log_in_batches()
        {
            for (var i = 0; i < 2500; i++)
            {
                await sut.LogAsync(new Request { Key = i.ToString() });
            }

            sut.Next();
            sut.Dispose();

            A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("0", "999")))
                .MustHaveHappened();

            A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("1000", "1999")))
                .MustHaveHappened();

            A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("2000", "2499")))
                .MustHaveHappened();
        }

        private static IEnumerable<Request> Batch(string from, string to)
        {
            return A<IEnumerable<Request>>.That.Matches(x => x.First().Key == from && x.Last().Key == to);
        }
    }
}
