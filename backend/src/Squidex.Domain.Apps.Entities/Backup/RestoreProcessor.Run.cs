// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Backup.State;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed partial class RestoreProcessor
{
    // Use a run to store all state that is necessary for a single run.
    private sealed class Run : IDisposable
    {
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly CancellationTokenSource cancellationLinked;

        public IEnumerable<IBackupHandler> Handlers { get; init; }

        public IBackupReader Reader { get; set; }

        public RestoreJob Job { get; init; }

        public RestoreContext Context { get; set; }

        public StreamMapper StreamMapper { get; set; }

        public CancellationToken CancellationToken => cancellationLinked.Token;

        public Run(CancellationToken ct)
        {
            cancellationLinked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationSource.Token);
        }

        public void Dispose()
        {
            Reader?.Dispose();

            cancellationSource.Dispose();
            cancellationLinked.Dispose();
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
}
