// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Web.Pipeline
{
    public sealed class CachingManager : IRequestCache
    {
        public const string SurrogateKeySizeHeader = "X-SurrogateKeys";
        private const int MaxAllowedKeysSize = 20000;
        private readonly ObjectPool<StringBuilder> stringBuilderPool;
        private readonly CachingOptions cachingOptions;
        private readonly IHttpContextAccessor httpContextAccessor;

        internal sealed class CacheContext : IDisposable
        {
            private readonly IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            private readonly HashSet<string> keys = new HashSet<string>();
            private readonly HashSet<string> headers = new HashSet<string>();
            private readonly ReaderWriterLockSlim slimLock = new ReaderWriterLockSlim();
            private readonly int maxKeysSize;
            private bool hasDependency;

            public CacheContext(int maxKeysSize)
            {
                this.maxKeysSize = maxKeysSize;
            }

            public void Dispose()
            {
                hasher.Dispose();

                slimLock.Dispose();
            }

            public void AddDependency(string key, long version)
            {
                if (key != default)
                {
                    try
                    {
                        slimLock.EnterWriteLock();

                        keys.Add(key);

                        hasher.AppendData(Encoding.Default.GetBytes(key));
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

            public void Finish(HttpResponse response, ObjectPool<StringBuilder> stringBuilderPool)
            {
                if (hasDependency && !response.Headers.ContainsKey(HeaderNames.ETag))
                {
                    using (Telemetry.Activities.StartActivity("CalculateEtag"))
                    {
                        var cacheBuffer = hasher.GetHashAndReset();
                        var cacheString = BitConverter.ToString(cacheBuffer).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

                        response.Headers.Add(HeaderNames.ETag, cacheString);
                    }
                }

                if (keys.Count > 0 && maxKeysSize > 0)
                {
                    var stringBuilder = stringBuilderPool.Get();
                    try
                    {
                        foreach (var key in keys)
                        {
                            if (stringBuilder.Length == 0)
                            {
                                if (stringBuilder.Length + key.Length > maxKeysSize)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (stringBuilder.Length + key.Length + 1 > maxKeysSize)
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
                        stringBuilderPool.Return(stringBuilder);
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

        public CachingManager(IHttpContextAccessor httpContextAccessor, IOptions<CachingOptions> cachingOptions)
        {
            this.httpContextAccessor = httpContextAccessor;

            this.cachingOptions = cachingOptions.Value;

            stringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy
            {
                MaximumRetainedCapacity = cachingOptions.Value.MaxSurrogateKeysSize
            });
        }

        public void Start(HttpContext httpContext)
        {
            Guard.NotNull(httpContext, nameof(httpContext));

            var maxKeysSize = GetKeysSize(httpContext);

            httpContext.Features.Set(new CacheContext(maxKeysSize));
        }

        private int GetKeysSize(HttpContext httpContext)
        {
            var headers = httpContext.Request.Headers;

            if (!headers.TryGetValue(SurrogateKeySizeHeader, out var header) || !int.TryParse(header, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size))
            {
                size = cachingOptions.MaxSurrogateKeysSize;
            }

            return Math.Min(MaxAllowedKeysSize, size);
        }

        public void AddDependency(DomainId key, long version)
        {
            var cacheContext = httpContextAccessor.HttpContext?.Features.Get<CacheContext>();

            cacheContext?.AddDependency(key.ToString(), version);
        }

        public void AddDependency(object? value)
        {
            var cacheContext = httpContextAccessor.HttpContext?.Features.Get<CacheContext>();

            cacheContext?.AddDependency(value);
        }

        public void AddHeader(string header)
        {
            var cacheContext = httpContextAccessor.HttpContext?.Features.Get<CacheContext>();

            cacheContext?.AddHeader(header);
        }

        public void Finish(HttpContext httpContext)
        {
            Guard.NotNull(httpContext, nameof(httpContext));

            var cacheContext = httpContext.Features.Get<CacheContext>();

            cacheContext?.Finish(httpContext.Response, stringBuilderPool);
        }
    }
}
