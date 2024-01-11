// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using Argon;
using NodaTime;
using NodaTime.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.TestHelpers;

public static partial class VerifySettings
{
    private sealed class JsonArrayConverter : WriteOnlyJsonConverter<JsonArray>
    {
        public override void Write(VerifyJsonWriter writer, JsonArray value)
        {
            writer.WriteStartArray();

            foreach (var item in value)
            {
                writer.Serialize(item);
            }

            writer.WriteEndArray();
        }
    }

    private sealed class JsonObjectConverter : WriteOnlyJsonConverter<JsonObject>
    {
        public override void Write(VerifyJsonWriter writer, JsonObject value)
        {
            writer.WriteStartObject();

            foreach (var (key, item) in value)
            {
                writer.WritePropertyName(key);
                writer.Serialize(item);
            }

            writer.WriteEndObject();
        }
    }

    private sealed class JsonValueConverter : WriteOnlyJsonConverter<JsonValue>
    {
        public override void Write(VerifyJsonWriter writer, JsonValue value)
        {
            switch (value.Type)
            {
                case JsonValueType.Null:
                    writer.WriteNull();
                    break;
                case JsonValueType.Array:
                    writer.Serialize(value.AsArray);
                    break;
                case JsonValueType.Boolean:
                    writer.WriteValue(value.AsBoolean);
                    break;
                case JsonValueType.Number:
                    writer.WriteValue(value.AsNumber);
                    break;
                case JsonValueType.Object:
                    writer.Serialize(value.AsObject);
                    break;
                case JsonValueType.String:
                    writer.WriteValue(value.AsString);
                    break;
            }
        }
    }

    private sealed class ContractResolver : IContractResolver
    {
        private readonly IContractResolver inner;

        public ContractResolver(IContractResolver inner)
        {
            this.inner = inner;
        }

        public JsonNameTable GetNameTable()
        {
            return inner.GetNameTable();
        }

        public JsonContract ResolveContract(Type type)
        {
            var contract = inner.ResolveContract(type);

            if (contract is not JsonDictionaryContract dictionaryContract)
            {
                return contract;
            }

            var originalKeyResolver = dictionaryContract.DictionaryKeyResolver!;

            dictionaryContract.DictionaryKeyResolver = (name, original) =>
            {
                if (original is string id && Guid.TryParse(id, out var guid1))
                {
                    var index = Counter.Current.Next(guid1);

                    return $"Guid_{index}";
                }

                if (original is DomainId id2 && Guid.TryParse(id2.ToString(), out var guid2))
                {
                    var index = Counter.Current.Next(guid2);

                    return $"Guid_{index}";
                }

                return originalKeyResolver(name, original);
            };

            return contract;
        }
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        DerivePathInfo((sourceFile, projectDirectory, type, method) =>
        {
            var path = Path.Combine(projectDirectory, "Verify");

            return new PathInfo(path, type.Name, method.Name);
        });

        VerifierSettings.AddExtraSettings(s =>
        {
            s.Converters.Add(new JsonArrayConverter());
            s.Converters.Add(new JsonObjectConverter());
            s.Converters.Add(new JsonValueConverter());
            s.ContractResolver = new ContractResolver(s.ContractResolver!);
        });

        VerifierSettings.ScrubInlineGuids();
        VerifierSettings.IgnoreMembersWithType<Instant>();
        VerifierSettings.IgnoreInstance<JsonValue>(x => x.Type == JsonValueType.String && InstantPattern.ExtendedIso.Parse(x.ToString()).Success);
        VerifierSettings.IgnoreMember("Secret");
    }
}
