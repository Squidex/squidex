// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Squidex.Infrastructure
{
    public static class GravatarHelper
    {
        public static string CreatePictureUrl(string email)
        {
            var gravatarUrl = $"https://www.gravatar.com/avatar/{Hash(email)}";

            return gravatarUrl;
        }

        public static string CreateProfileUrl(string email)
        {
            var gravatarUrl = $"https://www.gravatar.com/{Hash(email)}";

            return gravatarUrl;
        }

        private static string Hash(string email)
        {
            using (var md5 = MD5.Create())
            {
                var normalizedEmail = email.ToLowerInvariant().Trim();

                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(normalizedEmail));
                var hashBuilder = new StringBuilder();

                for (var i = 0; i < hashBytes.Length; i++)
                {
                    hashBuilder.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return hashBuilder.ToString();
            }
        }
    }
}
