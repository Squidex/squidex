// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Shared.Users
{
    public static class UserExtensions
    {
        public static PermissionSet Permissions(this IUser user)
        {
            return new PermissionSet(user.GetClaimValues(SquidexClaimTypes.Permissions).Select(x => new Permission(x)));
        }

        public static bool IsInvited(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.Invited, "true");
        }

        public static bool IsHidden(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.Hidden, "true");
        }

        public static bool HasConsent(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.Consent, "true");
        }

        public static bool HasConsentForEmails(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.ConsentForEmails, "true");
        }

        public static bool HasDisplayName(this IUser user)
        {
            return user.HasClaim(SquidexClaimTypes.DisplayName);
        }

        public static bool HasPictureUrl(this IUser user)
        {
            return user.HasClaim(SquidexClaimTypes.PictureUrl);
        }

        public static bool IsPictureUrlStored(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.PictureUrl, SquidexClaimTypes.PictureUrlStore);
        }

        public static string? ClientSecret(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.ClientSecret);
        }

        public static string? PictureUrl(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.PictureUrl);
        }

        public static string? DisplayName(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.DisplayName);
        }

        public static string? GetClaimValue(this IUser user, string type)
        {
            return user.Claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string[] GetClaimValues(this IUser user, string type)
        {
            return user.Claims.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value).ToArray();
        }

        public static List<(string Name, string Value)> GetCustomProperties(this IUser user)
        {
            return user.Claims.Where(x => x.Type.StartsWith(SquidexClaimTypes.CustomPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(x => (x.Type[(SquidexClaimTypes.CustomPrefix.Length + 1)..], x.Value)).ToList();
        }

        public static bool HasClaim(this IUser user, string type)
        {
            return user.Claims.Any(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasClaimValue(this IUser user, string type, string value)
        {
            return user.Claims.Any(x =>
                string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        public static string? PictureNormalizedUrl(this IUser user)
        {
            var url = user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.PictureUrl)?.Value;

            if (!string.IsNullOrWhiteSpace(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute) && url.Contains("gravatar"))
            {
                if (url.Contains("?"))
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
    }
}
