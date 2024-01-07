// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class JobRun : IDisposable
{
    private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
    private readonly CancellationTokenSource cancellationLinked;
    private readonly SimpleState<JobsState> state;

    public RefToken Actor { get; init; }

    public Job Job { get; init; }

    public DomainId OwnerId { get; init; }

    public CancellationToken CancellationToken => cancellationLinked.Token;

    public JobRun(SimpleState<JobsState> state, CancellationToken ct)
    {
        this.state = state;

        cancellationLinked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationSource.Token);
    }

    public void Dispose()
    {
        cancellationSource.Dispose();
        cancellationLinked.Dispose();
    }

    public Task LogAsync(string message, bool replace = false)
    {
        if (replace && Job.Log.Count > 0)
        {
            Job.Log[^1] = message;
        }
        else
        {
            Job.Log.Add(message);
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
