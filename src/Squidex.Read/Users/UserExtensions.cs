// ==========================================================================
//  UserExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using Squidex.Core.Identity;
using Squidex.Infrastructure;

namespace Squidex.Read.Users
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

        public static void SetPictureUrlFromGravatar(this IUser user, string email)
        {
            user.SetClaim(SquidexClaimTypes.SquidexPictureUrl, GravatarHelper.CreatePictureUrl(email));
        }

        public static string PictureUrl(this IUser user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexPictureUrl)?.Value;
        }

        public static string DisplayName(this IUser user)
        {
            return user.Claims.FirstOrDefault(x => x.Type == SquidexClaimTypes.SquidexDisplayName)?.Value;
        }
    }
}
