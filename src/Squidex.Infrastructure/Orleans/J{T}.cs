// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.CodeGeneration;
using Orleans.Concurrency;
using Orleans.Serialization;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Orleans
{
    [Immutable]
    public struct J<T>
    {
        public T Value { get; }

        [JsonConstructor]
        public J(T value)
        {
            Value = value;
        }

        public static implicit operator T(J<T> value)
        {
            return value.Value;
        }

        public static implicit operator J<T>(T d)
        {
            return new J<T>(d);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public static Task<J<T>> AsTask(T value)
        {
            return Task.FromResult<J<T>>(value);
        }

        [CopierMethod]
        public static object Copy(object input, ICopyContext context)
        {
            return input;
        }

        [SerializerMethod]
        public static void Serialize(object input, ISerializationContext context, Type expected)
        {
            using (Profiler.TraceMethod(nameof(J)))
            {
                var jsonSerializer = GetSerializer(context);

                var stream = new StreamWriterWrapper(context.StreamWriter);

                using (var writer = new JsonTextWriter(new StreamWriter(stream)))
                {
                    jsonSerializer.Serialize(writer, input);

                    writer.Flush();
                }
            }
        }

        [DeserializerMethod]
        public static object Deserialize(Type expected, IDeserializationContext context)
        {
            using (Profiler.TraceMethod(nameof(J)))
            {
                var jsonSerializer = GetSerializer(context);

                var stream = new StreamReaderWrapper(context.StreamReader);

                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    return jsonSerializer.Deserialize(reader, expected);
                }
            }
        }

        private static JsonSerializer GetSerializer(ISerializerContext context)
        {
            try
            {
                return context?.ServiceProvider?.GetService<JsonSerializer>() ?? J.DefaultSerializer;
            }
            catch
            {
                return J.DefaultSerializer;
            }
        }
    }
}
