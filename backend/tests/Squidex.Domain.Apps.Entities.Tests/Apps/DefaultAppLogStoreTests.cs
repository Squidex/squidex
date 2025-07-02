// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using NodaTime;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Apps;

public class DefaultAppLogStoreTests : GivenContext
{
    private readonly IRequestLogStore requestLogStore = A.Fake<IRequestLogStore>();
    private readonly DefaultAppLogStore sut;

    public DefaultAppLogStoreTests()
    {
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
        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => requestLogStore.DeleteAsync($"^[a-z]-{AppId.Id}", A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_forward_request_if_disabled()
    {
        A.CallTo(() => requestLogStore.IsEnabled)
            .Returns(false);

        await sut.LogAsync(AppId.Id, default, CancellationToken);

        A.CallTo(() => requestLogStore.LogAsync(A<Request>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_forward_request_log_to_store()
    {
        Request? recordedRequest = null;

        A.CallTo(() => requestLogStore.IsEnabled)
            .Returns(true);

        A.CallTo(() => requestLogStore.LogAsync(A<Request>._, CancellationToken))
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

        await sut.LogAsync(AppId.Id, request, CancellationToken);

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

        Assert.Equal(AppId.Id.ToString(), recordedRequest?.Key);
    }

    [Fact]
    public async Task Should_write_to_stream()
    {
        var timeFrom = SystemClock.Instance.GetCurrentInstant();
        var timeTo = timeFrom.Plus(Duration.FromDays(4));

        A.CallTo(() => requestLogStore.QueryAllAsync(AppId.Id.ToString(), timeFrom, timeTo, CancellationToken))
            .Returns(new[]
            {
                CreateRecord(),
                CreateRecord(),
                CreateRecord(),
                CreateRecord(),
            }.ToAsyncEnumerable());

        var stream = new MemoryStream();

        await sut.ReadLogAsync(AppId.Id, timeFrom, timeTo, stream, CancellationToken);
        stream.Position = 0;

        var lines = 0;
        using (var reader = new StreamReader(stream))
        {
            while (await reader.ReadLineAsync(default) != null)
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
/*
FAILED TEST: The test run failed due to **C# syntax errors** in the file `DefaultAppLogStoreTests.cs`, preventing successful compilation:

### Errors:
1. **Line 148**: Missing semicolon (`;`) at the end of a statement.
2. **Lines 393–396**: Invalid token `{` and incorrect code block placement, indicating a malformed method or class member.

### Recommended Fixes:
1. Add a missing semicolon at the end of the line indicated in the error (`line 148`).
2. Correct the syntax at `line 393` and following lines by ensuring:
   - Correct placement of braces `{}`.
   - Valid assignment or declaration syntax.

These fixes will resolve the compilation errors and allow the test to run.

    [Fact]
    public async Task Should_add_max_values_to_request()
    {
        Request? recordedRequest = null;
    
        A.CallTo(() => requestLogStore.IsEnabled)
            .Returns(true);
    
        A.CallTo(() => requestLogStore.LogAsync(A<Request>._, A<CancellationToken>._))
            .Invokes(x => recordedRequest = x.GetArgument<Request>(0)!);
    
        var request = new RequestLog
        {
            Bytes = long.MaxValue,
            CacheHits = long.MaxValue,
            CacheTTL = long.MaxValue,
            Costs = double.MaxValue,
            ElapsedMs = double.MaxValue,
            StatusCode = int.MaxValue,
            Timestamp = default
        };
    
        await sut.LogAsync(AppId.Id, request, CancellationToken.None);
    
        Assert.NotNull(recordedRequest);
    
        Assert.Contains(DefaultAppLogStore.FieldBytes, recordedRequest.Properties);
        Assert.Contains(DefaultAppLogStore.FieldCacheHits, recordedRequest.Properties);
        Assert.Contains(DefaultAppLogStore.FieldCacheTTL, recordedRequest.Properties);
        Assert.Contains(DefaultAppLogStore.FieldCosts, recordedRequest.Properties);
        Assert.Contains(DefaultAppLogStore.FieldRequestElapsedMs, recordedRequest.Properties);
        Assert.Contains(DefaultAppLogStore.FieldStatusCode, recordedRequest.Properties);
    }

*/
/*
FAILED TEST: The test run failed due to **C# syntax errors** in the file `DefaultAppLogStoreTests.cs`:

1. **Line 152**: A semicolon (`;`) is missing at the end of a statement.
2. **Lines 350–353**: There is an invalid token `{` and incorrect code block placement, indicating a malformed method or class member.

### Recommended Fixes:
1. Add a missing semicolon at the end of the line indicated in the error (`line 152`).
2. Correct the syntax at `line 350` and following lines by ensuring:
   - Correct placement of braces `{}`.
   - Valid assignment or declaration syntax.

    [Fact]
    public async Task Should_return_null_for_invalid_data_in_GetDouble_and_GetLong()
    {
        var timeFrom = SystemClock.Instance.GetCurrentInstant();
        var timeTo = timeFrom.Plus(Duration.FromDays(4));
    
        var request = new Request
        {
            Properties = new Dictionary<string, string>
            {
                { FieldRequestElapsedMs, "not a number" },
                { FieldCosts, "invalid" },
                { FieldBytes, "abc" },
                { FieldCacheHits, "xyz" },
                { FieldCacheTTL, "123.45" },
                { FieldStatusCode, "not a long" }
            }
        };
    
        A.CallTo(() => requestLogStore.QueryAllAsync(AppId.Id.ToString(), timeFrom, timeTo, CancellationToken.None))
            .Returns(new[] { request }.ToAsyncEnumerable());
    
        var stream = new MemoryStream();
    
        await sut.ReadLogAsync(AppId.Id, timeFrom, timeTo, stream, CancellationToken.None);
        stream.Position = 0;
    
        using (var reader = new StreamReader(stream))
        {
            var header = await reader.ReadLineAsync();
            var dataLine = await reader.ReadLineAsync();
    
            Assert.NotNull(header);
            Assert.NotNull(dataLine);
        }
    }

*/
/*
FAILED TEST: The test run failed due to **C# syntax errors** in the file `DefaultAppLogStoreTests.cs`:

1. **Line 154**: A semicolon (`;`) is missing at the end of a statement.
2. **Lines 300–303**: There is an invalid token `{` and incorrect code block placement, indicating a malformed method or class member.

### Recommended Fixes:
1. Add a missing semicolon at the end of the line indicated in the error (`line 154`).
2. Correct the syntax at `line 300` and following lines by ensuring:
   - Correct placement of braces `{}`.
   - Valid assignment or declaration syntax.

These fixes will resolve the compilation errors and allow the test to run.

    [Fact]
    public async Task Should_write_correct_number_of_records_to_stream()
    {
        var timeFrom = SystemClock.Instance.GetCurrentInstant();
        var timeTo = timeFrom.Plus(Duration.FromDays(4));
    
        A.CallTo(() => requestLogStore.QueryAllAsync(AppId.Id.ToString(), timeFrom, timeTo, CancellationToken))
            .Returns(Enumerable.Empty<Request>().ToAsyncEnumerable());
    
        var stream = new MemoryStream();
    
        await sut.ReadLogAsync(AppId.Id, timeFrom, timeTo, stream, CancellationToken);
        stream.Position = 0;
    
        var lines = 0;
        using (var reader = new StreamReader(stream))
        {
            while (await reader.ReadLineAsync() != null)
            {
                lines++;
            }
        }
    
        Assert.Equal(1, lines); // Only the header line
    
        A.CallTo(() => requestLogStore.QueryAllAsync(AppId.Id.ToString(), timeFrom, timeTo, CancellationToken))
            .Returns(new[] { CreateRecord() }.ToAsyncEnumerable());
    
        stream = new MemoryStream();
    
        await sut.ReadLogAsync(AppId.Id, timeFrom, timeTo, stream, CancellationToken);
        stream.Position = 0;
    
        lines = 0;
        using (var reader = new StreamReader(stream))
        {
            while (await reader.ReadLineAsync() != null)
            {
                lines++;
            }
        }
    
        Assert.Equal(2, lines); // Header + one data line
    }

*/
/*
FAILED TEST: The test run failed due to **C# syntax errors** in the test file `DefaultAppLogStoreTests.cs`:

1. **Line 147**: A semicolon (`;`) is missing at the end of a statement.
2. **Lines 183–186**: There is an invalid token `{` and incorrect code block placement, indicating a malformed method or class member.

### Recommended Fixes:
1. Add a missing semicolon at the end of the line indicated in the error (`line 147`).
2. Correct the syntax at `line 183` and following lines by ensuring:
   - Correct placement of braces `{}`.
   - Valid assignment or declaration syntax.

These fixes will resolve the compilation errors and allow the test to run.

    [Fact]
    public async Task Should_not_add_non_numeric_values_to_request()
    {
        Request? recordedRequest = null;
    
        A.CallTo(() => requestLogStore.IsEnabled)
            .Returns(true);
    
        A.CallTo(() => requestLogStore.LogAsync(A<Request>._, A<CancellationToken>._))
            .Invokes(x => recordedRequest = x.GetArgument<Request>(0)!);
    
        var request = new RequestLog
        {
            Bytes = "not a number",
            CacheHits = "invalid",
            CacheTTL = "abc",
            Costs = "xyz",
            ElapsedMs = "120.5",
            StatusCode = "not a long",
            Timestamp = default
        };
    
        await sut.LogAsync(AppId.Id, request, CancellationToken.None);
    
        Assert.NotNull(recordedRequest);
    
        Assert.DoesNotContain(FieldBytes, recordedRequest.Properties);
        Assert.DoesNotContain(FieldCacheHits, recordedRequest.Properties);
        Assert.DoesNotContain(FieldCacheTTL, recordedRequest.Properties);
        Assert.DoesNotContain(FieldCosts, recordedRequest.Properties);
        Assert.DoesNotContain(FieldRequestElapsedMs, recordedRequest.Properties);
        Assert.DoesNotContain(FieldStatusCode, recordedRequest.Properties);
    }

*/
/*
FAILED TEST: The test run failed due to **C# syntax errors** in the test file `DefaultAppLogStoreTests.cs`. The compiler errors indicate malformed code structure, specifically:

- **Line 147**: Missing semicolon (`;`) at the end of a statement.
- **Lines 183–186**: Invalid token `{` and incorrect placement of code block, suggesting a misplaced or improperly structured method or class member.

### Recommended Fixes:
1. Add a missing semicolon at the end of the line indicated in the error (`line 147`).
2. Correct the syntax at `line 183` and following lines by ensuring:
   - Proper method or class member declaration.
   - Correct placement of braces `{}`.
   - Valid assignment or declaration syntax.

These fixes will resolve the compilation errors and allow the test to run.

    [Fact]
    public async Task Should_not_add_null_or_empty_string_fields_to_request()
    {
        Request? recordedRequest = null;
    
        A.CallTo(() => requestLogStore.IsEnabled)
            .Returns(true);
    
        A.CallTo(() => requestLogStore.LogAsync(A<Request>._, A<CancellationToken>._))
            .Invokes(x => recordedRequest = x.GetArgument<Request>(0)!);
    
        var request = new RequestLog
        {
            UserClientId = null,
            UserId = string.Empty,
            RequestPath = null,
            RequestMethod = null,
            CacheServer = null,
            CacheStatus = string.Empty,
            Timestamp = default
        };
    
        await sut.LogAsync(AppId.Id, request, CancellationToken.None);
    
        Assert.NotNull(recordedRequest);
    
        Assert.DoesNotContain(FieldAuthClientId, recordedRequest.Properties);
        Assert.DoesNotContain(FieldAuthUserId, recordedRequest.Properties);
        Assert.DoesNotContain(FieldRequestPath, recordedRequest.Properties);
        Assert.DoesNotContain(FieldRequestMethod, recordedRequest.Properties);
        Assert.DoesNotContain(FieldCacheServer, recordedRequest.Properties);
        Assert.DoesNotContain(FieldCacheStatus, recordedRequest.Properties);
    }

*/
    {
        return new Request { Properties = [] };
    }
}
