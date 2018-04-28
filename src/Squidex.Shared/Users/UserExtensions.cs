// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;

namespace Squidex.Shared.Users
{
    public static class UserExtensions
    {
        public static void SetDisplayName(this IUser user, string displayName)
        {
            user.SetClaim(SquidexClaimTypes.SquidexDisplayName, displayName);
        }

        public static void SetPictureUrl(this IUser user, string pictureUrl)
        {
            user.SetClaim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl);
        }

        public static void SetPictureUrlToStore(this IUser user)
        {
            user.SetClaim(SquidexClaimTypes.SquidexPictureUrl, "store");
        }

        public static void SetPictureUrlFromGravatar(this IUser user, string email)
        {
            user.SetClaim(SquidexClaimTypes.SquidexPictureUrl, GravatarHelper.CreatePictureUrl(email));
        }

        public static void SetHidden(this IUser user, bool value)
        {
            user.SetClaim(SquidexClaimTypes.SquidexHidden, value.ToString());
        }

        public static void SetConsent(this IUser user)
        {
            user.SetClaim(SquidexClaimTypes.SquidexConsent, "true");
        }

        public static void SetConsentForEmails(this IUser user, bool value)
        {
            user.SetClaim(SquidexClaimTypes.SquidexConsentForEmails, value.ToString());
        }

        public static bool IsHidden(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.SquidexHidden, "true");
        }

        public static bool HasConsent(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.SquidexConsent, "true");
        }

        public static bool HasConsentForEmails(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.SquidexConsentForEmails, "true");
        }

        public static bool HasDisplayName(this IUser user)
        {
            return user.HasClaim(SquidexClaimTypes.SquidexDisplayName);
        }

        public static bool HasPictureUrl(this IUser user)
        {
            return user.HasClaim(SquidexClaimTypes.SquidexPictureUrl);
        }

        public static bool IsPictureUrlStored(this IUser user)
        {
            return user.HasClaimValue(SquidexClaimTypes.SquidexPictureUrl, "store");
        }

        public static string PictureUrl(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.SquidexPictureUrl);
        }

        public static string DisplayName(this IUser user)
        {
            return user.GetClaimValue(SquidexClaimTypes.SquidexDisplayName);
        }

        public static string GetClaimValue(this IUser user, string claim)
        {
            return user.Claims.FirstOrDefault(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        public static bool HasClaim(this IUser user, string claim)
        {
            return user.Claims.Any(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasClaimValue(this IUser user, string claim, string value)
        {
            return user.Claims.Any(x => string.Equals(x.Type, claim, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
        }

        public static string PictureNormalizedUrl(this IUser user)
        {
            var url = user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexPictureUrl)?.Value;

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
