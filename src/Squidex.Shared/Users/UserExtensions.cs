// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;

namespace Squidex.Shared.Users
{
    public static class UserExtensions
    {
        public static void SetDisplayName(this IUser user, string displayName)
        {
            user.SetClaim(SquidexClaimTypes.DisplayName, displayName);
        }

        public static void SetPictureUrl(this IUser user, string pictureUrl)
        {
            user.SetClaim(SquidexClaimTypes.PictureUrl, pictureUrl);
        }

        public static void SetPictureUrlToStore(this IUser user)
        {
            user.SetClaim(SquidexClaimTypes.PictureUrl, "store");
        }

        public static void SetPictureUrlFromGravatar(this IUser user, string email)
        {
            user.SetClaim(SquidexClaimTypes.PictureUrl, GravatarHelper.CreatePictureUrl(email));
        }

        public static void SetHidden(this IUser user, bool value)
        {
            user.SetClaim(SquidexClaimTypes.Hidden, value.ToString());
        }

        public static void SetConsent(this IUser user)
        {
            user.SetClaim(SquidexClaimTypes.Consent, "true");
        }

        public static void SetConsentForEmails(this IUser user, bool value)
        {
            user.SetClaim(SquidexClaimTypes.ConsentForEmails, value.ToString());
        }

        public static void SetPermissions(this IUser user, params string[] permissions)
        {
            user.RemoveClaims(SquidexClaimTypes.Permissions);

            foreach (var permission in permissions)
            {
                user.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }
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
            return user.HasClaimValue(SquidexClaimTypes.PictureUrl, "store");
        }

        public static string PictureUrl(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.PictureUrl);
        }

        public static string DisplayName(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.DisplayName);
        }

        public static string[] Permissions(this ClaimsPrincipal principal)
        {
            return principal.Claims.Where(x => x.Type == SquidexClaimTypes.Permissions).Select(x => x.Value).ToArray();
        }

        public static string[] Permissions(this IUser user)
        {
            return user.GetClaimValues(SquidexClaimTypes.Permissions);
        }

        public static string GetClaimValue(this IUser user, string type)
        {
            return user.Claims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static string[] GetClaimValues(this IUser user, string type)
        {
            return user.Claims.Where(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToArray();
        }

        public static bool HasClaim(this IUser user, string type)
        {
            return user.Claims.Any(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasClaimValue(this IUser user, string type, string value)
        {
            return user.Claims.Any(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        public static string PictureNormalizedUrl(this IUser user)
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
