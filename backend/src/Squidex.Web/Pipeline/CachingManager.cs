// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Web.Pipeline;

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
        private bool isFinished;

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
            if (!string.IsNullOrWhiteSpace(key))
            {
                slimLock.EnterWriteLock();
                try
                {
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

        public void AddDependency<T>(T value)
        {
            var formatted = value?.ToString();

            if (formatted != null)
            {
                slimLock.EnterWriteLock();
                try
                {
                    hasher.AppendData(Encoding.Default.GetBytes(formatted));

                    hasDependency = true;
                }
                finally
                {
                    slimLock.ExitWriteLock();
                }
            }
        }

        public void Finish(HttpResponse response, ObjectPool<StringBuilder> stringBuilderPool)
        {
            // Finish might be called multiple times.
            if (isFinished)
            {
                return;
            }

            // Set to finish before we start to ensure that we do not call it again in case of an error.
            isFinished = true;

            if (hasDependency && !response.Headers.ContainsKey(HeaderNames.ETag))
            {
                using (Telemetry.Activities.StartActivity("CalculateEtag"))
                {
                    var cacheBuffer = hasher.GetHashAndReset();
                    var cacheString = cacheBuffer.ToHexString();

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
                        var encoded = Uri.EscapeDataString(key);

                        if (stringBuilder.Length == 0)
                        {
                            if (stringBuilder.Length + encoded.Length > maxKeysSize)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (stringBuilder.Length + encoded.Length + 1 > maxKeysSize)
                            {
                                break;
                            }

                            stringBuilder.Append(' ');
                        }

                        stringBuilder.Append(encoded);
                    }

                    if (stringBuilder.Length > 0)
                    {
                        response.Headers["Surrogate-Key"] = stringBuilder.ToString();
                    }
                }
                finally
                {
                    stringBuilderPool.Return(stringBuilder);
                }
            }

            if (headers.Count > 0)
            {
                response.Headers[HeaderNames.Vary] = new StringValues(headers.ToArray());
            }
        }

        public void AddHeader(string header, StringValues values)
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

                foreach (var value in values)
                {
                    AddDependency(value);
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

    public void Reset(HttpContext httpContext)
    {
        Guard.NotNull(httpContext);

        httpContext.Features.Set<CacheContext>(null);
    }

    public void Start(HttpContext httpContext)
    {
        Guard.NotNull(httpContext);

        var maxKeysSize = GetKeysSize(httpContext);

        // Ensure that we only add the cache context once.
        if (httpContext.Features.Get<CacheContext>() != null)
        {
            return;
        }

        httpContext.Features.Set(new CacheContext(maxKeysSize));
    }

    public void Finish(HttpContext httpContext)
    {
        Guard.NotNull(httpContext);

        var cacheContext = httpContext.Features.Get<CacheContext>();

        // If the cache context has not been set it does not make sense to handle it now.
        if (cacheContext == null)
        {
            return;
        }

        cacheContext.Finish(httpContext.Response, stringBuilderPool);
    }

    private int GetKeysSize(HttpContext httpContext)
    {
        var headers = httpContext.Request.Headers;

        if (!headers.TryGetValue(SurrogateKeySizeHeader, out var header) || TryParseHeader(header, out var size))
        {
            size = cachingOptions.MaxSurrogateKeysSize;
        }

        return Math.Min(MaxAllowedKeysSize, size);
    }

    private static bool TryParseHeader(StringValues header, out int size)
    {
        return !int.TryParse(header, NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
    }

    public void AddDependency(DomainId key, long version)
    {
        var cacheContext = httpContextAccessor.HttpContext?.Features.Get<CacheContext>();

        // The cache context can be null if start has never been called.
        cacheContext?.AddDependency(key.ToString(), version);
    }

    public void AddDependency<T>(T value)
    {
        var cacheContext = httpContextAccessor.HttpContext?.Features.Get<CacheContext>();

        // The cache context can be null if start has never been called.
        cacheContext?.AddDependency(value);
    }

    public void AddHeader(string header)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            return;
        }

        var cacheContext = httpContext.Features.Get<CacheContext>();

        // The cache context can be null if start has never been called.
        if (cacheContext == null)
        {
            return;
        }

        httpContext.Request.Headers.TryGetValue(header, out var value);

        cacheContext?.AddHeader(header, value);
    }
}
