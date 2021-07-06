// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public static class AssetExtensions
    {
        private const string HeaderNoEnrichment = "X-NoAssetEnrichment";

        public static bool ShouldSkipAssetEnrichment(this Context context)
        {
            return context.Headers.ContainsKey(HeaderNoEnrichment);
        }

        public static ICloneBuilder WithoutAssetEnrichment(this ICloneBuilder builder, bool value = true)
        {
            return builder.WithBoolean(HeaderNoEnrichment, value);
        }

        public static async Task<string> GetTextAsync(this IAssetFileStore assetFileStore, DomainId appId, DomainId id, long fileVersion, string? encoding)
        {
            using (var stream = DefaultPools.MemoryStream.GetStream())
            {
                await assetFileStore.DownloadAsync(appId, id, fileVersion, null, stream);

                stream.Position = 0;

                var bytes = stream.ToArray();

                switch (encoding?.ToLowerInvariant())
                {
                    case "base64":
                        return Convert.ToBase64String(bytes);
                    case "ascii":
                        return Encoding.ASCII.GetString(bytes);
                    case "unicode":
                        return Encoding.Unicode.GetString(bytes);
                    default:
                        return Encoding.UTF8.GetString(bytes);
                }
            }
        }
    }
}
