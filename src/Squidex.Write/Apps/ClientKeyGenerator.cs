// ==========================================================================
//  ClientKeyGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Squidex.Write.Apps
{
    public class ClientKeyGenerator
    {
        public virtual string GenerateKey()
        {
            return Sha256(Guid.NewGuid().ToString());
        }

        private static string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hash).Replace("+", "x");
            }
        }
    }
}
