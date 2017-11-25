// ==========================================================================
//  BsonJsonReader.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NewtonsoftJsonReader = Newtonsoft.Json.JsonReader;
using NewtonsoftJsonToken = Newtonsoft.Json.JsonToken;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class BsonJsonReader : NewtonsoftJsonReader
    {
        private readonly IBsonReader bsonReader;

        public BsonJsonReader(IBsonReader bsonReader)
        {
            Guard.NotNull(bsonReader, nameof(bsonReader));

            this.bsonReader = bsonReader;
        }

        public override bool Read()
        {
            if (bsonReader.State == BsonReaderState.Type)
            {
                bsonReader.ReadBsonType();
            }

            if (bsonReader.State == BsonReaderState.Name)
            {
                SetToken(NewtonsoftJsonToken.PropertyName, bsonReader.ReadName().UnescapeBson());
            }
            else if (bsonReader.State == BsonReaderState.EndOfDocument)
            {
                SetToken(NewtonsoftJsonToken.EndObject);
                bsonReader.ReadEndDocument();
            }
            else if (bsonReader.State == BsonReaderState.EndOfArray)
            {
                SetToken(NewtonsoftJsonToken.EndArray);
                bsonReader.ReadEndArray();
            }
            else if (bsonReader.State == BsonReaderState.Value)
            {
                switch (bsonReader.CurrentBsonType)
                {
                    case BsonType.Document:
                        SetToken(NewtonsoftJsonToken.StartObject);
                        bsonReader.ReadStartDocument();
                        break;
                    case BsonType.Array:
                        SetToken(NewtonsoftJsonToken.StartArray);
                        bsonReader.ReadStartArray();
                        break;
                    case BsonType.Undefined:
                        SetToken(NewtonsoftJsonToken.Undefined);
                        break;
                    case BsonType.Null:
                        SetToken(NewtonsoftJsonToken.Null);
                        break;
                    case BsonType.String:
                        SetToken(NewtonsoftJsonToken.String, bsonReader.ReadString());
                        break;
                    case BsonType.Binary:
                        SetToken(NewtonsoftJsonToken.Bytes, bsonReader.ReadBinaryData().Bytes);
                        break;
                    case BsonType.Boolean:
                        SetToken(NewtonsoftJsonToken.Boolean, bsonReader.ReadBoolean());
                        break;
                    case BsonType.DateTime:
                        SetToken(NewtonsoftJsonToken.Date, bsonReader.ReadDateTime());
                        break;
                    case BsonType.Int32:
                        SetToken(NewtonsoftJsonToken.Integer, bsonReader.ReadInt32());
                        break;
                    case BsonType.Int64:
                        SetToken(NewtonsoftJsonToken.Integer, bsonReader.ReadInt64());
                        break;
                    case BsonType.Double:
                        SetToken(NewtonsoftJsonToken.Float, bsonReader.ReadDouble());
                        break;
                    case BsonType.Decimal128:
                        SetToken(NewtonsoftJsonToken.Float, Decimal128.ToDouble(bsonReader.ReadDecimal128()));
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

            return !bsonReader.IsAtEndOfFile();
        }
    }
}
