// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using Microsoft.AspNetCore.WebUtilities;

namespace Squidex.Web.GraphQL
{
    public sealed class BufferingGraphQLSerializer : IGraphQLTextSerializer
    {
        private readonly IGraphQLTextSerializer inner;

        public BufferingGraphQLSerializer(IGraphQLTextSerializer inner)
        {
            this.inner = inner;
        }

        public async ValueTask<T?> ReadAsync<T>(Stream stream,
            CancellationToken cancellationToken = default)
        {
            await using (var bufferStream = new FileBufferingReadStream(stream, 30 * 1024))
            {
                await bufferStream.DrainAsync(cancellationToken);

                bufferStream.Seek(0L, SeekOrigin.Begin);

                return await inner.ReadAsync<T>(bufferStream, cancellationToken);
            }
        }

        public async Task WriteAsync<T>(Stream stream, T? value,
            CancellationToken cancellationToken = default)
        {
            await using (var bufferStream = new FileBufferingWriteStream())
            {
                await inner.WriteAsync(bufferStream, value, cancellationToken);

                await bufferStream.DrainBufferAsync(stream, cancellationToken);
            }
        }

        public string Serialize<T>(T? value)
        {
            return inner.Serialize<T>(value);
        }

        public T? Deserialize<T>(string? value)
        {
            return inner.Deserialize<T>(value);
        }

        public T? ReadNode<T>(object? value)
        {
            return inner.ReadNode<T>(value);
        }
    }
}
