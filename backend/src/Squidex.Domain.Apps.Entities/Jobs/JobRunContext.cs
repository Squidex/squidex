// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class JobRunContext : IDisposable
{
    private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
    private readonly CancellationTokenSource cancellationLinked;
    private readonly SimpleState<JobsState> state;
    private readonly IClock clock;

    required public RefToken Actor { get; init; }

    required public Job Job { get; init; }

    required public DomainId OwnerId { get; init; }

    public CancellationToken CancellationToken => cancellationLinked.Token;

    public JobRunContext(SimpleState<JobsState> state, IClock clock, CancellationToken ct)
    {
        this.state = state;
        this.clock = clock;

        cancellationLinked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationSource.Token);
    }

    public void Dispose()
    {
        cancellationSource.Dispose();
        cancellationLinked.Dispose();
    }

    public string GetArgument(string name)
    {
        if (!Job.Arguments.TryGetValue(name, out var value))
        {
            throw new DomainException($"Argument '{name}' missing.");
        }

        return value;
    }

    public DomainId GetArgumentId(string name)
    {
        return DomainId.Create(GetArgument(name));
    }

    public string? TryGetArgument(string name)
    {
        return Job.Arguments.GetValueOrDefault(name);
    }

    public bool GetArgumentFlag(string name, bool defaultValue = false)
    {
        if (!Job.Arguments.TryGetValue(name, out var value) || !bool.TryParse(value, out var result))
        {
            return defaultValue;
        }

        return result;
    }

    public Task LogAsync(string message, bool replace = false)
    {
        var item = new JobLogMessage(clock.GetCurrentInstant(), message);

        if (replace && Job.Log.Count > 0)
        {
            Job.Log[^1] = item;
        }
        else
        {
            Job.Log.Add(item);
        }

        return state.WriteAsync(100, CancellationToken);
    }

    public Task FlushAsync()
    {
        return state.WriteAsync(CancellationToken);
    }

    public void Cancel()
    {
        try
        {
            cancellationSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Cancellation token might have been disposed, if the run is completed.
        }
    }
}
