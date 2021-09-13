// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Cryptography;
using System.Text;

namespace Squidex.Infrastructure
{
    public static class RandomHash
    {
        public static string New()
        {
            return Guid.NewGuid()
                .ToString().ToSha256Base64()
                .ToLowerInvariant()
                .Replace("+", "x", StringComparison.Ordinal)
                .Replace("=", "x", StringComparison.Ordinal)
                .Replace("/", "x", StringComparison.Ordinal);
        }

        public static string Simple()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal);
        }

        public static string ToSha256Base64(this string value)
        {
            return ToSha256Base64(Encoding.UTF8.GetBytes(value));
        }

        public static string ToSha256Base64(this byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var bytesHash = sha.ComputeHash(bytes);

                var result = Convert.ToBase64String(bytesHash);

                return result;
            }
        }

        public static string ToSha256(this string value)
        {
            return value.ToHashed(SHA256.Create());
        }

        public static string ToSha256(this byte[] bytes)
        {
            return bytes.ToHashed(SHA256.Create());
        }

        public static string ToMD5(this string value)
        {
            return value.ToHashed(MD5.Create());
        }

        public static string ToMD5(this byte[] bytes)
        {
            return bytes.ToHashed(MD5.Create());
        }

        public static string ToHashed(this string value, HashAlgorithm algorithm)
        {
            return Encoding.UTF8.GetBytes(value).ToHashed(algorithm);
        }

        public static string ToHashed(this byte[] bytes, HashAlgorithm algorithm)
        {
            using (algorithm)
            {
                var bytesHash = algorithm.ComputeHash(bytes);

                var result = Encoding.UTF8.GetString(bytesHash);

                return result;
            }
        }
    }
}
