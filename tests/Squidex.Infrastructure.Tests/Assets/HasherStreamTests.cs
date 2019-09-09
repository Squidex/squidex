// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Squidex.Infrastructure.Assets
{
    public class HasherStreamTests
    {
        [Fact]
        public void Should_calculate_hash_while_copying()
        {
            var source = GenerateTestData();
            var sourceHash = source.Sha256Base64();

            var sourceStream = new HasherStream(new MemoryStream(source), HashAlgorithmName.SHA256);

            using (sourceStream)
            {
                var target = new MemoryStream();

                sourceStream.CopyTo(target);

                var targetHash = sourceStream.GetHashStringAndReset();

                Assert.Equal(sourceHash, targetHash);
            }
        }

        private static byte[] GenerateTestData(int length = 1000)
        {
            var random = new Random();
            var result = new byte[length];

            random.NextBytes(result);

            return result;
        }
    }
}
