// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class DefaultAppLogStore : IAppLogStore, IDeleter
    {
        private const string FieldAuthClientId = "AuthClientId";
        private const string FieldAuthUserId = "AuthUserId";
        private const string FieldBytes = "Bytes";
        private const string FieldCosts = "Costs";
        private const string FieldCacheStatus = "CacheStatus";
        private const string FieldCacheServer = "CacheServer";
        private const string FieldCacheTTL = "CacheTTL";
        private const string FieldCacheHits = "CacheHits";
        private const string FieldRequestElapsedMs = "RequestElapsedMs";
        private const string FieldRequestMethod = "RequestMethod";
        private const string FieldRequestPath = "RequestPath";
        private const string FieldStatusCode = "StatusCode";
        private const string FieldTimestamp = "Timestamp";

        private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            DetectDelimiter = false,
            Delimiter = "|",
            LeaveOpen = true
        };

        private readonly IRequestLogStore requestLogStore;

        public DefaultAppLogStore(IRequestLogStore requestLogStore)
        {
            this.requestLogStore = requestLogStore;
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return requestLogStore.DeleteAsync(app.Id.ToString(), ct);
        }

        public Task LogAsync(DomainId appId, RequestLog request,
            CancellationToken ct = default)
        {
            if (!requestLogStore.IsEnabled)
            {
                return Task.CompletedTask;
            }

            var storedRequest = new Request
            {
                Key = appId.ToString(),
                Timestamp = request.Timestamp
            };

            Append(storedRequest, FieldAuthClientId, request.UserClientId);
            Append(storedRequest, FieldAuthUserId, request.UserId);
            Append(storedRequest, FieldBytes, request.Bytes);
            Append(storedRequest, FieldCacheHits, request.CacheHits);
            Append(storedRequest, FieldCacheServer, request.CacheServer);
            Append(storedRequest, FieldCacheStatus, request.CacheStatus);
            Append(storedRequest, FieldCacheTTL, request.CacheTTL);
            Append(storedRequest, FieldCosts, request.Costs);
            Append(storedRequest, FieldRequestElapsedMs, request.ElapsedMs);
            Append(storedRequest, FieldRequestMethod, request.RequestMethod);
            Append(storedRequest, FieldRequestPath, request.RequestPath);
            Append(storedRequest, FieldStatusCode, request.StatusCode);

            return requestLogStore.LogAsync(storedRequest, ct);
        }

        public async Task ReadLogAsync(DomainId appId, DateTime fromDate, DateTime toDate, Stream stream,
            CancellationToken ct = default)
        {
            Guard.NotNull(appId, nameof(appId));

            var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true);
            try
            {
                await using (var csv = new CsvWriter(writer, CsvConfiguration))
                {
                    csv.WriteField(FieldTimestamp);
                    csv.WriteField(FieldRequestPath);
                    csv.WriteField(FieldRequestMethod);
                    csv.WriteField(FieldRequestElapsedMs);
                    csv.WriteField(FieldCosts);
                    csv.WriteField(FieldAuthClientId);
                    csv.WriteField(FieldAuthUserId);
                    csv.WriteField(FieldBytes);
                    csv.WriteField(FieldCacheHits);
                    csv.WriteField(FieldCacheServer);
                    csv.WriteField(FieldCacheStatus);
                    csv.WriteField(FieldCacheTTL);
                    csv.WriteField(FieldStatusCode);

                    await csv.NextRecordAsync();

                    await foreach (var request in requestLogStore.QueryAllAsync(appId.ToString(), fromDate, toDate, ct))
                    {
                        csv.WriteField(request.Timestamp.ToString());
                        csv.WriteField(GetString(request, FieldRequestPath));
                        csv.WriteField(GetString(request, FieldRequestMethod));
                        csv.WriteField(GetDouble(request, FieldRequestElapsedMs));
                        csv.WriteField(GetDouble(request, FieldCosts));
                        csv.WriteField(GetString(request, FieldAuthClientId));
                        csv.WriteField(GetString(request, FieldAuthUserId));
                        csv.WriteField(GetLong(request, FieldBytes));
                        csv.WriteField(GetLong(request, FieldCacheHits));
                        csv.WriteField(GetString(request, FieldCacheServer));
                        csv.WriteField(GetString(request, FieldCacheStatus));
                        csv.WriteField(GetLong(request, FieldCacheTTL));
                        csv.WriteField(GetLong(request, FieldStatusCode));

                        await csv.NextRecordAsync();
                    }
                }
            }
            finally
            {
                await writer.FlushAsync();
            }
        }

        private static void Append(Request request, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[key] = value;
            }
        }

        private static void Append(Request request, string key, object? value)
        {
            if (value != null)
            {
                Append(request, key, Convert.ToString(value, CultureInfo.InvariantCulture));
            }
        }

        private static string? GetString(Request request, string key)
        {
            return request.Properties.GetValueOrDefault(key, string.Empty);
        }

        private static double? GetDouble(Request request, string key)
        {
            if (request.Properties.TryGetValue(key, out var value) &&
                double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        private static long? GetLong(Request request, string key)
        {
            if (request.Properties.TryGetValue(key, out var value) &&
                long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
