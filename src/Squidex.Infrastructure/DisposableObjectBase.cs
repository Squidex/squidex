﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class DisposableObjectBase : IDisposable
    {
        private readonly object disposeLock = new object();
        private bool isDisposed;

        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            lock (disposeLock)
            {
                if (!isDisposed)
                {
                    DisposeObject(disposing);
                }
            }

            isDisposed = true;
        }

        protected abstract void DisposeObject(bool disposing);

        protected void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
