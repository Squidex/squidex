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
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
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
        public async Task Should_not_forward_request_if_disabled()
        {
            A.CallTo(() => requestLogStore.IsEnabled)
                .Returns(false);

            await sut.LogAsync(DomainId.NewGuid(), default);

            A.CallTo(() => requestLogStore.LogAsync(A<Request>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_forward_request_log_to_store()
        {
            Request? recordedRequest = null;

            A.CallTo(() => requestLogStore.IsEnabled)
                .Returns(true);

            A.CallTo(() => requestLogStore.LogAsync(A<Request>._))
                .Invokes((Request request) => recordedRequest = request);

            var request = default(RequestLog);
            request.Bytes = 1024;
            request.Costs = 1.5;
            request.ElapsedMs = 120;
            request.RequestMethod = "GET";
            request.RequestPath = "/my-path";
            request.Timestamp = default;
            request.UserClientId = "frontend";
            request.UserId = "user1";

            await sut.LogAsync(DomainId.NewGuid(), request);

            Assert.NotNull(recordedRequest);

            Assert.Contains(request.Bytes.ToString(CultureInfo.InvariantCulture), recordedRequest!.Properties.Values);
            Assert.Contains(request.Costs.ToString(CultureInfo.InvariantCulture), recordedRequest!.Properties.Values);
            Assert.Contains(request.ElapsedMs.ToString(), recordedRequest!.Properties.Values);
            Assert.Contains(request.RequestMethod, recordedRequest!.Properties.Values);
            Assert.Contains(request.RequestPath, recordedRequest!.Properties.Values);
            Assert.Contains(request.UserClientId, recordedRequest!.Properties.Values);
            Assert.Contains(request.UserId, recordedRequest!.Properties.Values);
        }

        [Fact]
        public async Task Should_create_some_stream()
        {
            var dateFrom = DateTime.UtcNow.Date.AddDays(-30);
            var dateTo = DateTime.UtcNow.Date;

            var appId = DomainId.NewGuid();

            A.CallTo(() => requestLogStore.QueryAllAsync(A<Func<Request, Task>>._, appId.ToString(), dateFrom, dateTo, default))
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
                while (await reader.ReadLineAsync() != null)
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
