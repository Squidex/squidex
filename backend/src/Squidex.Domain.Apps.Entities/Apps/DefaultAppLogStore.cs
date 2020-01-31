﻿// ==========================================================================
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
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log.Store;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class DefaultAppLogStore : IAppLogStore
    {
        private const string FieldAuthClientId = "AuthClientId";
        private const string FieldAuthUserId = "AuthUserId";
        private const string FieldCosts = "Costs";
        private const string FieldRequestElapsedMs = "RequestElapsedMs";
        private const string FieldRequestMethod = "RequestMethod";
        private const string FieldRequestPath = "RequestPath";
        private const string FieldTimestamp = "Timestamp";
        private readonly CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "|" };
        private readonly IRequestLogStore requestLogStore;

        public DefaultAppLogStore(IRequestLogStore requestLogStore)
        {
            Guard.NotNull(requestLogStore);

            this.requestLogStore = requestLogStore;
        }

        public Task LogAsync(Guid appId, Instant timestamp, string? requestMethod, string? requestPath, string? userId, string? clientId, long elapsedMs, double costs)
        {
            var request = new Request
            {
                Key = appId.ToString(),
                Properties = new Dictionary<string, string>
                {
                    [FieldCosts] = costs.ToString()
                },
                Timestamp = timestamp
            };

            Append(request, FieldAuthClientId, clientId);
            Append(request, FieldAuthUserId, userId);
            Append(request, FieldCosts, costs.ToString(CultureInfo.InvariantCulture));
            Append(request, FieldRequestElapsedMs, elapsedMs.ToString(CultureInfo.InvariantCulture));
            Append(request, FieldRequestMethod, requestMethod);
            Append(request, FieldRequestPath, requestPath);

            return requestLogStore.LogAsync(request);
        }

        public async Task ReadLogAsync(Guid appId, DateTime fromDate, DateTime toDate, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNull(appId);

            var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true);
            try
            {
                using (var csv = new CsvWriter(writer, csvConfiguration, true))
                {
                    csv.WriteField(FieldTimestamp);
                    csv.WriteField(FieldRequestPath);
                    csv.WriteField(FieldRequestMethod);
                    csv.WriteField(FieldRequestElapsedMs);
                    csv.WriteField(FieldCosts);
                    csv.WriteField(FieldAuthClientId);
                    csv.WriteField(FieldAuthUserId);

                    await csv.NextRecordAsync();

                    await requestLogStore.QueryAllAsync(async request =>
                    {
                        csv.WriteField(request.Timestamp.ToString());
                        csv.WriteField(GetString(request, FieldRequestPath));
                        csv.WriteField(GetString(request, FieldRequestMethod));
                        csv.WriteField(GetDouble(request, FieldRequestElapsedMs));
                        csv.WriteField(GetDouble(request, FieldCosts));
                        csv.WriteField(GetString(request, FieldAuthClientId));
                        csv.WriteField(GetString(request, FieldAuthUserId));

                        await csv.NextRecordAsync();
                    }, appId.ToString(), fromDate, toDate, ct);
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

        private static string GetString(Request request, string key)
        {
            return request.Properties.GetValueOrDefault(key, string.Empty)!;
        }

        private static double GetDouble(Request request, string key)
        {
            if (request.Properties.TryGetValue(key, out var value) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return 0;
        }
    }
}
