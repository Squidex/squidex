// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Cryptography;
using System.Text;

namespace TestSuite.Utils
{
    public static class RandomHash
    {
        public static string New()
        {
            return Guid.NewGuid()
                .ToString().Sha256Base64()
                .ToLowerInvariant()
                .Replace("+", "x")
                .Replace("=", "x")
                .Replace("/", "x");
        }

        public static string Simple()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public static string Sha256Base64(this string value)
        {
            return Sha256Base64(Encoding.UTF8.GetBytes(value));
        }

        public static string Sha256Base64(this byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var bytesHash = sha.ComputeHash(bytes);

                var result = Convert.ToBase64String(bytesHash);

                return result;
            }
        }
    }
}
