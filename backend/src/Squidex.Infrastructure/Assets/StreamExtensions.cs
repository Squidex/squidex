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

        public static async Task CopyToAsync(this Stream source, Stream target, BytesRange range, CancellationToken ct, bool skip = true)
        {
            var buffer = Pool.Rent(8192);

            try
            {
                if (skip && range.From > 0)
                {
                    source.Seek(range.From.Value, SeekOrigin.Begin);
                }

                var bytesLeft = range.Length;

                while (true)
                {
                    if (bytesLeft <= 0)
                    {
                        return;
                    }

                    ct.ThrowIfCancellationRequested();

                    var readLength = (int)Math.Min(buffer.Length, bytesLeft);

                    var read = await source.ReadAsync(buffer, 0, readLength, ct);

                    bytesLeft -= read;

                    if (read == 0)
                    {
                        return;
                    }

                    ct.ThrowIfCancellationRequested();

                    await target.WriteAsync(buffer, 0, read, ct);
                }
            }
            finally
            {
                Pool.Return(buffer);
            }
        }
    }
}
