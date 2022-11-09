// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Infrastructure;

#pragma warning disable MA0040 // Flow the cancellation token

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed partial class BackupProcessor
{
    // Use a run to store all state that is necessary for a single run.
    private sealed class Run : IDisposable
    {
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly CancellationTokenSource cancellationLinked;

        public IEnumerable<IBackupHandler> Handlers { get; init; }

        public RefToken Actor { get; init; }

        public BackupJob Job { get; init; }

        public CancellationToken CancellationToken => cancellationLinked.Token;

        public Run(CancellationToken ct)
        {
            cancellationLinked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationSource.Token);
        }

        public void Dispose()
        {
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
