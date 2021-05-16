// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using OpenIddict.Abstractions;
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Config
{
    public static class ApplicationExtensions
    {
        public static string BuildId(string value)
        {
            const int MongoDbLength = 24;

            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(value);

            foreach (var c in bytes)
            {
                sb.Append(c.ToString("X2"));

                if (sb.Length == MongoDbLength)
                {
                    break;
                }
            }

            while (sb.Length < MongoDbLength)
            {
                sb.Append('0');
            }

            return sb.ToString();
        }

        public static OpenIddictApplicationDescriptor CopyClaims(this OpenIddictApplicationDescriptor application, IUser claims)
        {
            foreach (var group in claims.Claims.GroupBy(x => x.Type))
            {
                application.Properties[group.Key] = (JsonElement)new OpenIddictParameter(group.Select(x => x.Value).ToArray());
            }

            return application;
        }

        public static IEnumerable<Claim> Claims(this IReadOnlyDictionary<string, JsonElement> properties)
        {
            foreach (var (key, value) in properties)
            {
                var values = (string[]?)new OpenIddictParameter(value);

                if (values != null)
                {
                    foreach (var claimValue in values)
                    {
                        yield return new Claim(key, claimValue);
                    }
                }
            }
        }
    }
}
