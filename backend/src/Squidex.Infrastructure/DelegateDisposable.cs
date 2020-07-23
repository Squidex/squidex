// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public sealed class DelegateDisposable : IDisposable
    {
        private readonly Action action;

        public DelegateDisposable(Action action)
        {
            Guard.NotNull(action, nameof(action));

            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}
