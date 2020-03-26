// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public static class StreamExtensions
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();

        public static async Task CopyToAsync(this Stream source, Stream target, Range range, CancellationToken ct, bool skip = true)
        {
            var buffer = Pool.Rent(8192);

            try
            {
                if (skip && range.Offset > 0)
                {
                    source.Seek(range.Offset, SeekOrigin.Begin);
                }

                var bytesLeft = range.Length > 0 ? range.Length : long.MaxValue;
                int bytesRead;

                while (bytesLeft > 0 && !ct.IsCancellationRequested && (bytesRead = source.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesLeft))) > 0)
                {
                    await target.WriteAsync(buffer, 0, bytesRead);

                    bytesLeft -= bytesRead;
                }
            }
            finally
            {
                Pool.Return(buffer);
            }
        }
    }
}
