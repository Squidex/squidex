// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed record Component(string Type, JsonObject Data, Schema Schema)
    {
        public const string Discriminator = "schemaId";

        public string Type { get; } = Guard.NotNullOrEmpty(Type);

        public Schema Schema { get; } = Guard.NotNull(Schema);

        public JsonObject Data { get; } = Guard.NotNull(Data);

        public static bool IsValid(JsonValue value, [MaybeNullWhen(false)] out string discriminator)
        {
            discriminator = null!;

            if (value.Type != JsonValueType.Object)
            {
                return false;
            }

            if (!value.AsObject.TryGetValue(Discriminator, out var type) || type.Type != JsonValueType.String)
            {
                return false;
            }

            var typed = type.AsString;

            if (string.IsNullOrWhiteSpace(typed))
            {
                return false;
            }

            discriminator = typed;

            return true;
        }
    }
}
