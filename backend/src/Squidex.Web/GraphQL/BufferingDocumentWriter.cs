// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.NewtonsoftJson;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Squidex.Web.GraphQL
{
    public sealed class BufferingDocumentWriter : IDocumentWriter
    {
        private readonly DocumentWriter documentWriter;

        public BufferingDocumentWriter(Action<JsonSerializerSettings> action)
        {
            documentWriter = new DocumentWriter(action);
        }

        public async Task WriteAsync<T>(Stream stream, T value,
            CancellationToken cancellationToken = default)
        {
            await using (var bufferStream = new FileBufferingWriteStream())
            {
                await documentWriter.WriteAsync(bufferStream, value, cancellationToken);

                await bufferStream.DrainBufferAsync(stream, cancellationToken);
            }
        }
    }
}
