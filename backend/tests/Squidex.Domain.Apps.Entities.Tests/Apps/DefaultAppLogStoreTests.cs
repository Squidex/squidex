// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log.Store;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class DefaultAppLogStoreTests
    {
        private readonly IRequestLogStore requestLogStore = A.Fake<IRequestLogStore>();
        private readonly DefaultAppLogStore sut;

        public DefaultAppLogStoreTests()
        {
            sut = new DefaultAppLogStore(requestLogStore);
        }

        [Fact]
        public async Task Should_forward_request_log_to_store()
        {
            Request? recordedRequest = null;

            A.CallTo(() => requestLogStore.LogAsync(A<Request>.Ignored))
                .Invokes((Request request) => recordedRequest = request);

            var clientId = "frontend";
            var costs = 2;
            var elapsedMs = 120;
            var requestMethod = "GET";
            var requestPath = "/my-path";
            var userId = "user1";

            await sut.LogAsync(Guid.NewGuid(), default, requestMethod, requestPath, userId, clientId, elapsedMs, costs);

            Assert.NotNull(recordedRequest);

            Assert.Contains(clientId, recordedRequest!.Properties.Values);
            Assert.Contains(costs.ToString(), recordedRequest!.Properties.Values);
            Assert.Contains(elapsedMs.ToString(), recordedRequest!.Properties.Values);
            Assert.Contains(requestMethod, recordedRequest!.Properties.Values);
            Assert.Contains(requestPath, recordedRequest!.Properties.Values);
        }

        [Fact]
        public async Task Should_create_some_stream()
        {
            var dateFrom = DateTime.UtcNow.Date.AddDays(-30);
            var dateTo = DateTime.UtcNow.Date;

            var appId = Guid.NewGuid();

            A.CallTo(() => requestLogStore.QueryAllAsync(A<Func<Request, Task>>.Ignored, appId.ToString(), dateFrom, dateTo, default))
                .Invokes(x =>
                {
                    var callback = x.GetArgument<Func<Request, Task>>(0)!;

                    callback(CreateRecord());
                    callback(CreateRecord());
                    callback(CreateRecord());
                    callback(CreateRecord());
                });

            var stream = new MemoryStream();

            await sut.ReadLogAsync(appId, dateFrom, dateTo, stream);

            stream.Position = 0;

            var lines = 0;

            using (var reader = new StreamReader(stream))
            {
                string? line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    lines++;
                }
            }

            Assert.Equal(5, lines);
        }

        private static Request CreateRecord()
        {
            return new Request { Properties = new Dictionary<string, string>() };
        }
    }
}
