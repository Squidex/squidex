// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;

namespace Squidex.Infrastructure.Tasks
{
    public sealed class AsyncLocalCleaner<T> : IDisposable
    {
        private readonly AsyncLocal<T> asyncLocal;

        public AsyncLocalCleaner(AsyncLocal<T> asyncLocal)
        {
            Guard.NotNull(asyncLocal, nameof(asyncLocal));

            this.asyncLocal = asyncLocal;
        }

        public void Dispose()
        {
            asyncLocal.Value = default(T);
        }
    }
}
