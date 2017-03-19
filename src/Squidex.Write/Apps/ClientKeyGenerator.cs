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
        private readonly Func<HashAlgorithm> algorithmFactory;

        public ClientKeyGenerator()
        {
            algorithmFactory = SHA256.Create;
        }

        public virtual string GenerateKey()
        {
            return Hash(Guid.NewGuid().ToString());
        }

        private string Hash(string input)
        {
            using (var sha = algorithmFactory())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hash).Replace("+", "x");
            }
        }
    }
}
