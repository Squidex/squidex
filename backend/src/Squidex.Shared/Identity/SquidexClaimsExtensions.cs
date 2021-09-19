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
using System.Security.Claims;
using Squidex.Infrastructure.Security;

namespace Squidex.Shared.Identity
{
    public static class SquidexClaimsExtensions
    {
        private const string ClientPrefix = "client_";

        public static PermissionSet Permissions(this IEnumerable<Claim> user)
        {
            var permissions = user.GetClaims(SquidexClaimTypes.Permissions).Select(x => x.Value);

            return new PermissionSet(permissions);
        }

        public static bool IsHidden(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(SquidexClaimTypes.Hidden, "true");
        }

        public static bool HasConsent(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(SquidexClaimTypes.Consent, "true");
        }

        public static bool HasConsentForEmails(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(SquidexClaimTypes.ConsentForEmails, "true");
        }

        public static bool HasDisplayName(this IEnumerable<Claim> user)
        {
            return user.HasClaim(SquidexClaimTypes.DisplayName);
        }

        public static bool HasPictureUrl(this IEnumerable<Claim> user)
        {
            return user.HasClaim(SquidexClaimTypes.PictureUrl);
        }

        public static bool IsPictureUrlStored(this IEnumerable<Claim> user)
        {
            return user.HasClaimValue(SquidexClaimTypes.PictureUrl, SquidexClaimTypes.PictureUrlStore);
        }

        public static string? ClientSecret(this IEnumerable<Claim> user)
        {
            return user.GetClaimValue(SquidexClaimTypes.ClientSecret);
        }

        public static string? PictureUrl(this IEnumerable<Claim> user)
        {
            return user.GetClaimValue(SquidexClaimTypes.PictureUrl);
        }

        public static string? DisplayName(this IEnumerable<Claim> user)
        {
            return user.GetClaimValue(SquidexClaimTypes.DisplayName);
        }

        public static int GetTotalApps(this IEnumerable<Claim> user)
        {
            var value = user.GetClaimValue(SquidexClaimTypes.TotalApps);

            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);

            return result;
        }

        public static bool HasClaim(this IEnumerable<Claim> user, string type)
        {
            return user.GetClaims(type).Any();
        }

        public static bool HasClaimValue(this IEnumerable<Claim> user, string type, string value)
        {
            return user.GetClaims(type).Any(x => string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Claim> GetSquidexClaims(this IEnumerable<Claim> user)
        {
            const string prefix = "urn:squidex:";

            foreach (var claim in user)
            {
                var type = GetType(claim);

                if (type.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    yield return claim;
                }
            }
        }

        public static IEnumerable<(string Name, string Value)> GetCustomProperties(this IEnumerable<Claim> user)
        {
            foreach (var claim in user)
            {
                var type = GetType(claim);

                if (type.StartsWith(SquidexClaimTypes.CustomPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var name = type[(SquidexClaimTypes.CustomPrefix.Length + 1)..].ToString();

                    yield return (name, claim.Value);
                }
            }
        }

        public static string? PictureNormalizedUrl(this IEnumerable<Claim> user)
        {
            var url = user.FirstOrDefault(x => x.Type == SquidexClaimTypes.PictureUrl)?.Value;

            if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute) && url.Contains("gravatar", StringComparison.Ordinal))
            {
                if (url.Contains("?", StringComparison.Ordinal))
                {
                    url += "&d=404";
                }
                else
                {
                    url += "?d=404";
                }
            }

            return url;
        }

        private static string? GetClaimValue(this IEnumerable<Claim> user, string type)
        {
            return user.GetClaims(type).FirstOrDefault()?.Value;
        }

        private static IEnumerable<Claim> GetClaims(this IEnumerable<Claim> user, string request)
        {
            foreach (var claim in user)
            {
                var type = GetType(claim);

                if (type.Equals(request, StringComparison.OrdinalIgnoreCase))
                {
                    yield return claim;
                }
            }
        }

        private static ReadOnlySpan<char> GetType(Claim claim)
        {
            var type = claim.Type.AsSpan();

            if (type.StartsWith(ClientPrefix, StringComparison.OrdinalIgnoreCase))
            {
                type = type[ClientPrefix.Length..];
            }

            return type;
        }
    }
}
