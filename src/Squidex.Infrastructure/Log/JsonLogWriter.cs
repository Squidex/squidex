// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Newtonsoft.Json;
using NodaTime;

namespace Squidex.Infrastructure.Log
{
    public sealed class JsonLogWriter : IObjectWriter, IArrayWriter
    {
        private readonly Formatting formatting;
        private readonly bool formatLine;
        private readonly StringWriter textWriter = new StringWriter();
        private JsonWriter jsonWriter;

        public int BufferSize
        {
            get { return textWriter.GetStringBuilder().Capacity; }
        }

        internal JsonLogWriter(Formatting formatting, bool formatLine)
        {
            this.formatLine = formatLine;
            this.formatting = formatting;

            Start();
        }

        private void Start()
        {
            jsonWriter = new JsonTextWriter(textWriter) { Formatting = formatting };
            jsonWriter.WriteStartObject();
        }

        internal void Reset()
        {
            textWriter.GetStringBuilder().Clear();

            Start();
        }

        IArrayWriter IArrayWriter.WriteValue(string value)
        {
            jsonWriter.WriteValue(value);

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(double value)
        {
            jsonWriter.WriteValue(value);

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(long value)
        {
            jsonWriter.WriteValue(value);

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(bool value)
        {
            jsonWriter.WriteValue(value);

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(Instant value)
        {
            jsonWriter.WriteValue(value.ToString());

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(TimeSpan value)
        {
            jsonWriter.WriteValue(value.ToString());

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, string value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, double value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, long value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, bool value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, Instant value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value.ToString());

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, TimeSpan value)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteValue(value.ToString());

            return this;
        }

        IObjectWriter IObjectWriter.WriteObject(string property, Action<IObjectWriter> objectWriter)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteStartObject();

            objectWriter?.Invoke(this);

            jsonWriter.WriteEndObject();

            return this;
        }

        IObjectWriter IObjectWriter.WriteObject<T>(string property, T context, Action<T, IObjectWriter> objectWriter)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteStartObject();

            objectWriter?.Invoke(context, this);

            jsonWriter.WriteEndObject();

            return this;
        }

        IObjectWriter IObjectWriter.WriteArray(string property, Action<IArrayWriter> arrayWriter)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteStartArray();

            arrayWriter?.Invoke(this);

            jsonWriter.WriteEndArray();

            return this;
        }

        IObjectWriter IObjectWriter.WriteArray<T>(string property, T context, Action<T, IArrayWriter> arrayWriter)
        {
            jsonWriter.WritePropertyName(Format(property));
            jsonWriter.WriteStartArray();

            arrayWriter?.Invoke(context, this);

            jsonWriter.WriteEndArray();

            return this;
        }

        IArrayWriter IArrayWriter.WriteObject(Action<IObjectWriter> objectWriter)
        {
            jsonWriter.WriteStartObject();

            objectWriter?.Invoke(this);

            jsonWriter.WriteEndObject();

            return this;
        }

        IArrayWriter IArrayWriter.WriteObject<T>(T context, Action<T, IObjectWriter> objectWriter)
        {
            jsonWriter.WriteStartObject();

            objectWriter?.Invoke(context, this);

            jsonWriter.WriteEndObject();

            return this;
        }

        private static string Format(string property)
        {
            if (ReferenceEquals(string.IsInterned(property), property))
            {
                return property;
            }

            return property.ToCamelCase();
        }

        public override string ToString()
        {
            jsonWriter.WriteEndObject();

            if (formatLine)
            {
                textWriter.WriteLine();
            }

            return textWriter.ToString();
        }
    }
}
