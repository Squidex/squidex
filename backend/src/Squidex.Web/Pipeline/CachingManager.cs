// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Log;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingManager : IRequestCache
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        internal sealed class CacheContext : IRequestCache, IDisposable
        {
            private readonly IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            private readonly HashSet<string> keys = new HashSet<string>();
            private readonly ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim();
            private bool hasDependency;

            public void Dispose()
            {
                hasher.Dispose();

                slimLock.Dispose();
            }

            public void AddDependency(Guid key, long version)
            {
                if (key != default)
                {
                    try
                    {
                        slimLock.EnterWriteLock();

                        keys.Add(key.ToString());

                        hasher.AppendData(key.ToByteArray());
                        hasher.AppendData(BitConverter.GetBytes(version));

                        hasDependency = true;
                    }
                    finally
                    {
                        slimLock.ExitWriteLock();
                    }
                }
            }

            public void AddDependency(object? value)
            {
                if (value != null)
                {
                    try
                    {
                        slimLock.EnterWriteLock();

                        var formatted = value.ToString();

                        if (formatted != null)
                        {
                            hasher.AppendData(Encoding.Default.GetBytes(formatted));
                        }
                    }
                    finally
                    {
                        slimLock.ExitWriteLock();
                    }
                }
            }

            public void Finish(HttpResponse response, int maxSurrogateKeys)
            {
                if (hasDependency && !response.Headers.ContainsKey(HeaderNames.ETag))
                {
                    using (Profiler.Trace("CalculateEtag"))
                    {
                        var cacheBuffer = hasher.GetHashAndReset();
                        var cacheString = BitConverter.ToString(cacheBuffer).Replace("-", string.Empty).ToUpperInvariant();

                        response.Headers.Add(HeaderNames.ETag, cacheString);
                    }
                }

                if (keys.Count <= maxSurrogateKeys)
                {
                    var value = string.Join(" ", keys);

                    response.Headers.Add("Surrogate-Key", value);
                }
            }
        }

        public CachingManager(IHttpContextAccessor httpContextAccessor)
        {
            Guard.NotNull(httpContextAccessor);

            this.httpContextAccessor = httpContextAccessor;
        }

        public void Start(HttpContext httpContext)
        {
            Guard.NotNull(httpContext);

            httpContext.Features.Set(new CacheContext());
        }

        public void AddDependency(Guid key, long version)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var cacheContext = httpContextAccessor.HttpContext.Features.Get<CacheContext>();

                if (cacheContext != null)
                {
                    cacheContext.AddDependency(key, version);
                }
            }
        }

        public void AddDependency(object? value)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var cacheContext = httpContextAccessor.HttpContext.Features.Get<CacheContext>();

                if (cacheContext != null)
                {
                    cacheContext.AddDependency(value);
                }
            }
        }

        public void Finish(HttpContext httpContext, int maxSurrogateKeys)
        {
            Guard.NotNull(httpContext);

            var cacheContext = httpContextAccessor.HttpContext.Features.Get<CacheContext>();

            if (cacheContext != null)
            {
                cacheContext.Finish(httpContext.Response, maxSurrogateKeys);
            }
        }
    }
}
