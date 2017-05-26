// ==========================================================================
//  GravatarHelper.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Cryptography;
using System.Text;

namespace Squidex.Infrastructure
{
    public static class GravatarHelper
    {
        public static string CreatePictureUrl(string email)
        {
            using (var md5 = MD5.Create())
            {
                var gravatarHash = md5.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant().Trim()));
                var gravatarUrl = $"https://www.gravatar.com/avatar/{gravatarHash}";

                return gravatarUrl;
            }
        }
    }
}
