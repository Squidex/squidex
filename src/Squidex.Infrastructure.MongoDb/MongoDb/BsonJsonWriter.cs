﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using MongoDB.Bson.IO;
using NewtonsoftJSonWriter = Newtonsoft.Json.JsonWriter;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class BsonJsonWriter : NewtonsoftJSonWriter
    {
        private readonly IBsonWriter bsonWriter;

        public BsonJsonWriter(IBsonWriter bsonWriter)
        {
            Guard.NotNull(bsonWriter);

            this.bsonWriter = bsonWriter;
        }

        public override void WritePropertyName(string name)
        {
            bsonWriter.WriteName(name.EscapeJson());
        }

        public override void WritePropertyName(string name, bool escape)
        {
            bsonWriter.WriteName(name.EscapeJson());
        }

        public override void WriteStartArray()
        {
            bsonWriter.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            bsonWriter.WriteEndArray();
        }

        public override void WriteStartObject()
        {
            bsonWriter.WriteStartDocument();
        }

        public override void WriteEndObject()
        {
            bsonWriter.WriteEndDocument();
        }

        public override void WriteNull()
        {
            bsonWriter.WriteNull();
        }

        public override void WriteUndefined()
        {
            bsonWriter.WriteUndefined();
        }

        public override void WriteValue(string value)
        {
            bsonWriter.WriteString(value);
        }

        public override void WriteValue(int value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(uint value)
        {
            bsonWriter.WriteInt32((int)value);
        }

        public override void WriteValue(long value)
        {
            bsonWriter.WriteInt64(value);
        }

        public override void WriteValue(ulong value)
        {
            bsonWriter.WriteInt64((long)value);
        }

        public override void WriteValue(float value)
        {
            bsonWriter.WriteDouble(value);
        }

        public override void WriteValue(double value)
        {
            bsonWriter.WriteDouble(value);
        }

        public override void WriteValue(bool value)
        {
            bsonWriter.WriteBoolean(value);
        }

        public override void WriteValue(short value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(ushort value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(char value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(byte value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(sbyte value)
        {
            bsonWriter.WriteInt32(value);
        }

        public override void WriteValue(decimal value)
        {
            bsonWriter.WriteDecimal128(value);
        }

        public override void WriteValue(DateTime value)
        {
            bsonWriter.WriteString(value.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
        }

        public override void WriteValue(DateTimeOffset value)
        {
            if (value.Offset == TimeSpan.Zero)
            {
                bsonWriter.WriteString(value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
            }
            else
            {
                bsonWriter.WriteString(value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
            }
        }

        public override void WriteValue(byte[] value)
        {
            bsonWriter.WriteBytes(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            bsonWriter.WriteString(value.ToString());
        }

        public override void WriteValue(Guid value)
        {
            bsonWriter.WriteString(value.ToString());
        }

        public override void WriteValue(Uri value)
        {
            bsonWriter.WriteString(value.ToString());
        }

        public override void Flush()
        {
            bsonWriter.Flush();
        }
    }
}
