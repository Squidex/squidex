// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Log
{
    public sealed class JsonLogWriter : IObjectWriter, IArrayWriter
    {
        private readonly bool extraLine;
        private readonly StringWriter textWriter = new StringWriter();
        private readonly JsonWriter jsonWriter;

        public JsonLogWriter(Formatting formatting = Formatting.None, bool extraLine = false)
        {
            this.extraLine = extraLine;

            jsonWriter = new JsonTextWriter(textWriter) { Formatting = formatting };
            jsonWriter.WriteStartObject();
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

        IArrayWriter IArrayWriter.WriteValue(DateTime value)
        {
            jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(DateTimeOffset value)
        {
            jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));

            return this;
        }

        IArrayWriter IArrayWriter.WriteValue(TimeSpan value)
        {
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, string value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, double value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, long value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, bool value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, DateTime value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, DateTimeOffset value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));

            return this;
        }

        IObjectWriter IObjectWriter.WriteProperty(string property, TimeSpan value)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteValue(value);

            return this;
        }

        IObjectWriter IObjectWriter.WriteObject(string property, Action<IObjectWriter> objectWriter)
        {
            jsonWriter.WritePropertyName(property);
            jsonWriter.WriteStartObject();

            objectWriter?.Invoke(this);

            jsonWriter.WriteEndObject();

            return this;
        }

        IObjectWriter IObjectWriter.WriteArray(string property, Action<IArrayWriter> arrayWriter)
        {
            jsonWriter.WritePropertyName(property.ToCamelCase());
            jsonWriter.WriteStartArray();

            arrayWriter?.Invoke(this);

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

        public override string ToString()
        {
            jsonWriter.WriteEndObject();

            var result = textWriter.ToString();

            if (extraLine)
            {
                result += Environment.NewLine;
            }

            return result;
        }
    }
}
