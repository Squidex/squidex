// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class ClaimsPrincipalConverter : JsonClassConverter<ClaimsPrincipal>
    {
        private sealed class JsonIdentity
        {
            [JsonProperty]
            public string AuthenticationType { get; set; }

            [JsonProperty]
            public JsonClaim[] Claims { get; set; }
        }

        private sealed class JsonClaim
        {
            [JsonProperty]
            public string Type { get; set; }

            [JsonProperty]
            public string Value { get; set; }
        }

        protected override void WriteValue(JsonWriter writer, ClaimsPrincipal value, JsonSerializer serializer)
        {
            var jsonIdentities =
                value.Identities.Select(identity =>
                    new JsonIdentity
                    {
                        Claims = identity.Claims.Select(c =>
                        {
                            return new JsonClaim { Type = c.Type, Value = c.Value };
                        }).ToArray(),
                        AuthenticationType = identity.AuthenticationType
                    }).ToArray();

            serializer.Serialize(writer, jsonIdentities);
        }

        protected override ClaimsPrincipal ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var jsonIdentities = serializer.Deserialize<JsonIdentity[]>(reader);

            return new ClaimsPrincipal(
                jsonIdentities.Select(identity =>
                    new ClaimsIdentity(
                        identity.Claims.Select(c => new Claim(c.Type, c.Value)),
                        identity.AuthenticationType)));
        }
    }
}
