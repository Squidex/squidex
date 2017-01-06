// ==========================================================================
//  EnumExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public abstract class DisposableObject : IDisposable
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

            if (disposing)
            {
                lock (disposeLock)
                {
                    if (!isDisposed)
                    {
                        DisposeObject(true);
                    }
                }
            }
            else
            {
                DisposeObject(false);
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
