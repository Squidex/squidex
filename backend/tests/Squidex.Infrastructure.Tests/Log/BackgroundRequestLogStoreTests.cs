// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Log;

public class BackgroundRequestLogStoreTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IRequestLogRepository requestLogRepository = A.Fake<IRequestLogRepository>();
    private readonly RequestLogStoreOptions options = new RequestLogStoreOptions { StoreEnabled = true };
    private readonly BackgroundRequestLogStore sut;

    public BackgroundRequestLogStoreTests()
    {
        ct = cts.Token;

        sut = new BackgroundRequestLogStore(Options.Create(options), requestLogRepository, A.Fake<ILogger<BackgroundRequestLogStore>>());
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
    public async Task Should_forward_delete_call()
    {
        await sut.DeleteAsync("my-key", ct);

        A.CallTo(() => requestLogRepository.DeleteAsync("my-key", ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_log_if_disabled()
    {
        options.StoreEnabled = false;

        for (var i = 0; i < 2500; i++)
        {
            await sut.LogAsync(new Request { Key = i.ToString(CultureInfo.InvariantCulture) }, ct);
        }

        await WaitForCompletion();

        A.CallTo(() => requestLogRepository.InsertManyAsync(A<IEnumerable<Request>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_provide_results_from_repository()
    {
        var key = "my-key";

        var dateFrom = DateTime.Today;
        var dateTo = dateFrom.AddDays(4);

        A.CallTo(() => requestLogRepository.QueryAllAsync(key, dateFrom, dateTo, ct))
            .Returns(AsyncEnumerable.Repeat(new Request { Key = key }, 1));

        var actuals = await sut.QueryAllAsync(key, dateFrom, dateTo, ct).ToListAsync(ct);

        Assert.NotEmpty(actuals);
    }

    [Fact]
    public async Task Should_not_provide_results_from_repository_if_disabled()
    {
        options.StoreEnabled = false;

        var key = "my-key";

        var dateFrom = DateTime.Today;
        var dateTo = dateFrom.AddDays(4);

        var actuals = await sut.QueryAllAsync(key, dateFrom, dateTo, ct).ToListAsync(ct);

        Assert.Empty(actuals);

        A.CallTo(() => requestLogRepository.QueryAllAsync(key, dateFrom, dateTo, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_write_logs_in_batches()
    {
        for (var i = 0; i < 2500; i++)
        {
            await sut.LogAsync(new Request { Key = i.ToString(CultureInfo.InvariantCulture) }, ct);
        }

        await WaitForCompletion();

        A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("0", "999"), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("1000", "1999"), A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => requestLogRepository.InsertManyAsync(Batch("2000", "2499"), A<CancellationToken>._))
            .MustHaveHappened();
    }

    private async Task WaitForCompletion()
    {
        sut.Next();

        using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(5000));

        while (sut.HasPendingJobs)
        {
            tcs.Token.ThrowIfCancellationRequested();

            await Task.Delay(20, tcs.Token);
        }

        sut.Dispose();
    }

    private static IEnumerable<Request> Batch(string from, string to)
    {
        return A<IEnumerable<Request>>.That.Matches(x => x.First().Key == from && x.Last().Key == to);
    }
}
