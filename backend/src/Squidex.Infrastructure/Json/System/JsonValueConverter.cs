// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using SquidexJsonArray = Squidex.Infrastructure.Json.Objects.JsonArray;
using SquidexJsonObject = Squidex.Infrastructure.Json.Objects.JsonObject;
using SquidexJsonValue = Squidex.Infrastructure.Json.Objects.JsonValue;

namespace Squidex.Infrastructure.Json.System;

public sealed class JsonValueConverter : JsonConverter<SquidexJsonValue>
{
    public override bool HandleNull => true;

    public override SquidexJsonValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return SquidexJsonValue.True;
            case JsonTokenType.False:
                return SquidexJsonValue.False;
            case JsonTokenType.Null:
                return SquidexJsonValue.Null;
            case JsonTokenType.Number:
                return SquidexJsonValue.Create(reader.GetDouble());
            case JsonTokenType.String:
                return SquidexJsonValue.Create(reader.GetString());
            case JsonTokenType.StartObject:
                return JsonSerializer.Deserialize<SquidexJsonObject>(ref reader, options);
            case JsonTokenType.StartArray:
                return JsonSerializer.Deserialize<SquidexJsonArray>(ref reader, options);
            default:
                ThrowHelper.NotSupportedException();
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, SquidexJsonValue value, JsonSerializerOptions options)
    {
        switch (value.Value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case double n:
                writer.WriteNumberValue(n);
                break;
            case SquidexJsonArray a:
                JsonSerializer.Serialize(writer, a, options);
                break;
            case SquidexJsonObject o:
                JsonSerializer.Serialize(writer, o, options);
                break;
            default:
                ThrowHelper.NotSupportedException();
                break;
        }
    }
}
