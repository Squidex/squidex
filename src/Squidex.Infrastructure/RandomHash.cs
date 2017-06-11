// ==========================================================================
//  RandomHash.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
                var hash = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hash).Replace("+", "x");
            }
        }
    }
}
