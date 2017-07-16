// ==========================================================================
//  UserExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

// ReSharper disable InvertIf

namespace Squidex.Domain.Users
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

        public static bool IsPictureUrlStored(this IUser user)
        {
            return string.Equals(user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexPictureUrl)?.Value, "store", StringComparison.OrdinalIgnoreCase);
        }

        public static string PictureUrl(this IUser user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexPictureUrl)?.Value;
        }

        public static string DisplayName(this IUser user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexDisplayName)?.Value;
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
