// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.AspNetCore.WebUtilities;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class DefaultDocumentWriter : IDocumentWriter
    {
        private readonly IJsonSerializer jsonSerializer;

        public DefaultDocumentWriter(IJsonSerializer jsonSerializer)
        {
            this.jsonSerializer = jsonSerializer;
        }

        public async Task WriteAsync<T>(Stream stream, T value,
            CancellationToken cancellationToken = default)
        {
            await using (var buffer = new FileBufferingWriteStream())
            {
                jsonSerializer.Serialize(value, buffer, true);

                await buffer.DrainBufferAsync(stream, cancellationToken);
            }
        }
    }
}
