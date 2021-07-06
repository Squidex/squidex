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
    public sealed class DefaultAppLogStore : IAppLogStore
    {
        private const string FieldAuthClientId = "AuthClientId";
        private const string FieldAuthUserId = "AuthUserId";
        private const string FieldBytes = "Bytes";
        private const string FieldCosts = "Costs";
        private const string FieldRequestElapsedMs = "RequestElapsedMs";
        private const string FieldRequestMethod = "RequestMethod";
        private const string FieldRequestPath = "RequestPath";
        private const string FieldTimestamp = "Timestamp";

        private static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            LeaveOpen = true,
            LineBreakInQuotedFieldIsBadData = false
        };

        private readonly IRequestLogStore requestLogStore;

        public DefaultAppLogStore(IRequestLogStore requestLogStore)
        {
            this.requestLogStore = requestLogStore;
        }

        public Task LogAsync(DomainId appId, RequestLog request)
        {
            if (!requestLogStore.IsEnabled)
            {
                return Task.CompletedTask;
            }

            var storedRequest = new Request
            {
                Key = appId.ToString(),
                Properties = new Dictionary<string, string>
                {
                    [FieldCosts] = request.Costs.ToString(CultureInfo.InvariantCulture)
                },
                Timestamp = request.Timestamp
            };

            Append(storedRequest, FieldAuthClientId, request.UserClientId);
            Append(storedRequest, FieldAuthUserId, request.UserId);
            Append(storedRequest, FieldBytes, request.Bytes);
            Append(storedRequest, FieldCosts, request.Costs);
            Append(storedRequest, FieldRequestElapsedMs, request.ElapsedMs);
            Append(storedRequest, FieldRequestMethod, request.RequestMethod);
            Append(storedRequest, FieldRequestPath, request.RequestPath);

            return requestLogStore.LogAsync(storedRequest);
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
                        csv.WriteField(GetString(request, FieldBytes));

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

        private static void Append(Request request, string key, double value)
        {
            request.Properties[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        private static void Append(Request request, string key, long value)
        {
            request.Properties[key] = value.ToString(CultureInfo.InvariantCulture);
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
