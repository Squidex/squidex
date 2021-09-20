// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class DefaultAppLogStoreTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IRequestLogStore requestLogStore = A.Fake<IRequestLogStore>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DefaultAppLogStore sut;

        public DefaultAppLogStoreTests()
        {
            ct = cts.Token;

            sut = new DefaultAppLogStore(requestLogStore);
        }

        [Fact]
        public void Should_run_deletion_in_default_order()
        {
            var order = ((IDeleter)sut).Order;

            Assert.Equal(0, order);
        }

        [Fact]
        public async Task Should_remove_events_from_streams()
        {
            var app = Mocks.App(NamedId.Of(appId, "my-app"));

            await ((IDeleter)sut).DeleteAppAsync(app, ct);

            A.CallTo(() => requestLogStore.DeleteAsync($"^[a-z]-{app.Id}", ct))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_forward_request_if_disabled()
        {
            A.CallTo(() => requestLogStore.IsEnabled)
                .Returns(false);

            await sut.LogAsync(appId, default, ct);

            A.CallTo(() => requestLogStore.LogAsync(A<Request>._, ct))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_request_log_to_store()
        {
            Request? recordedRequest = null;

            A.CallTo(() => requestLogStore.IsEnabled)
                .Returns(true);

            A.CallTo(() => requestLogStore.LogAsync(A<Request>._, ct))
                .Invokes(x => recordedRequest = x.GetArgument<Request>(0)!);

            var request = default(RequestLog);
            request.Bytes = 1024;
            request.CacheHits = 10;
            request.CacheServer = "server-fra";
            request.CacheStatus = "MISS";
            request.CacheTTL = 3600;
            request.Costs = 1.5;
            request.ElapsedMs = 120;
            request.RequestMethod = "GET";
            request.RequestPath = "/my-path";
            request.StatusCode = 200;
            request.Timestamp = default;
            request.UserClientId = "frontend";
            request.UserId = "user1";

            await sut.LogAsync(appId, request, ct);

            Assert.NotNull(recordedRequest);

            Contains(request.Bytes, recordedRequest);
            Contains(request.CacheHits, recordedRequest);
            Contains(request.CacheServer, recordedRequest);
            Contains(request.CacheStatus, recordedRequest);
            Contains(request.CacheTTL, recordedRequest);
            Contains(request.ElapsedMs.ToString(CultureInfo.InvariantCulture), recordedRequest);
            Contains(request.RequestMethod, recordedRequest);
            Contains(request.RequestPath, recordedRequest);
            Contains(request.StatusCode, recordedRequest);
            Contains(request.UserClientId, recordedRequest);
            Contains(request.UserId, recordedRequest);

            Assert.Equal(appId.ToString(), recordedRequest?.Key);
        }

        [Fact]
        public async Task Should_write_to_stream()
        {
            var dateFrom = DateTime.UtcNow.Date.AddDays(-30);
            var dateTo = DateTime.UtcNow.Date;

            A.CallTo(() => requestLogStore.QueryAllAsync(appId.ToString(), dateFrom, dateTo, ct))
                .Returns(new[]
                {
                    CreateRecord(),
                    CreateRecord(),
                    CreateRecord(),
                    CreateRecord()
                }.ToAsyncEnumerable());

            var stream = new MemoryStream();

            await sut.ReadLogAsync(appId, dateFrom, dateTo, stream, ct);

            stream.Position = 0;

            var lines = 0;

            using (var reader = new StreamReader(stream))
            {
                while (await reader.ReadLineAsync() != null)
                {
                    lines++;
                }
            }

            Assert.Equal(5, lines);
        }

        private static void Contains(string value, Request? request)
        {
            Assert.Contains(value, request!.Properties.Values);
        }

        private static void Contains(object value, Request? request)
        {
            Assert.Contains(Convert.ToString(value, CultureInfo.InvariantCulture), request!.Properties.Values);
        }

        private static Request CreateRecord()
        {
            return new Request { Properties = new Dictionary<string, string>() };
        }
    }
}
