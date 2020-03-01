// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
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
            private static readonly ObjectPool<StringBuilder> StringBuilderPool =
                new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

            private readonly IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            private readonly HashSet<string> keys = new HashSet<string>();
            private readonly HashSet<string> headers = new HashSet<string>();
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

            public void Finish(HttpResponse response, int maxSurrogateKeySize)
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

                if (keys.Count > 0 && maxSurrogateKeySize > 0)
                {
                    const int GuidLength = 36;

                    var stringBuilder = StringBuilderPool.Get();
                    try
                    {
                        foreach (var key in keys)
                        {
                            if (stringBuilder.Length == 0)
                            {
                                if (stringBuilder.Length + GuidLength > maxSurrogateKeySize)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (stringBuilder.Length + GuidLength + 1 > maxSurrogateKeySize)
                                {
                                    break;
                                }

                                stringBuilder.Append(' ');
                            }

                            stringBuilder.Append(key);
                        }

                        if (stringBuilder.Length > 0)
                        {
                            response.Headers.Add("Surrogate-Key", stringBuilder.ToString());
                        }
                    }
                    finally
                    {
                        StringBuilderPool.Return(stringBuilder);
                    }
                }

                if (headers.Count > 0)
                {
                    response.Headers.Add(HeaderNames.Vary, new StringValues(headers.ToArray()));
                }
            }

            public void AddHeader(string header)
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    try
                    {
                        slimLock.EnterWriteLock();

                        headers.Add(header);
                    }
                    finally
                    {
                        slimLock.ExitWriteLock();
                    }
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

        public void AddHeader(string header)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var cacheContext = httpContextAccessor.HttpContext.Features.Get<CacheContext>();

                if (cacheContext != null)
                {
                    cacheContext.AddHeader(header);
                }
            }
        }

        public void Finish(HttpContext httpContext, int maxSurrogateKeySize)
        {
            Guard.NotNull(httpContext);

            var cacheContext = httpContextAccessor.HttpContext.Features.Get<CacheContext>();

            if (cacheContext != null)
            {
                cacheContext.Finish(httpContext.Response, maxSurrogateKeySize);
            }
        }
    }
}
