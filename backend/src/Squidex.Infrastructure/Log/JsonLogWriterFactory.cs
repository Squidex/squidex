// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Text.Json;

namespace Squidex.Infrastructure.Log
{
    public sealed class JsonLogWriterFactory : IObjectWriterFactory
    {
        private const int MaxPoolSize = 10;
        private const int MaxCapacity = 5000;
        private readonly ConcurrentStack<JsonLogWriter> pool = new ConcurrentStack<JsonLogWriter>();
        private readonly JsonWriterOptions formatting;
        private readonly bool formatLine;

        public JsonLogWriterFactory(bool indended = false, bool formatLine = false)
        {
            formatting.Indented = indended;

            this.formatLine = formatLine;
        }

        public static JsonLogWriterFactory Default()
        {
            return new JsonLogWriterFactory();
        }

        public static JsonLogWriterFactory Readable()
        {
            return new JsonLogWriterFactory(true, true);
        }

        public IObjectWriter Create()
        {
            if (pool.TryPop(out var writer))
            {
                writer.Reset();
            }
            else
            {
                writer = new JsonLogWriter(formatting, formatLine);
            }

            return writer;
        }

        public void Release(IObjectWriter writer)
        {
            var jsonWriter = (JsonLogWriter)writer;

            if (pool.Count < MaxPoolSize && jsonWriter.BufferSize < MaxCapacity)
            {
                pool.Push(jsonWriter);
            }
        }
    }
}
