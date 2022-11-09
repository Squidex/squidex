// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

public sealed class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
    private readonly Disposable disposable;

    private sealed class Disposable : IDisposable
    {
        private readonly SemaphoreSlim semaphore;

        public Disposable(SemaphoreSlim semaphore)
        {
            this.semaphore = semaphore;
        }

        public void Dispose()
        {
            semaphore.Release();
        }
    }

    public AsyncLock()
    {
        disposable = new Disposable(semaphore);
    }

    public async Task<IDisposable> EnterAsync(
        CancellationToken ct = default)
    {
        await semaphore.WaitAsync(ct);

        return disposable;
    }

    public IDisposable Enter()
    {
        semaphore.Wait();

        return disposable;
    }

    public void Dispose()
    {
        semaphore.Dispose();
    }
}
