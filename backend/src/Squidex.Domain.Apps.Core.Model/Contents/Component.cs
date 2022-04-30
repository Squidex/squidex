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

        public JsonObject Data { get; } = Guard.NotNull(Data);

        public Schema Schema { get; } = Guard.NotNull(Schema);

        public static bool IsValid(IJsonValue? value, [MaybeNullWhen(false)] out string discriminator)
        {
            discriminator = null!;

            if (value is not JsonObject obj)
            {
                return false;
            }

            if (!obj.TryGetValue<JsonString>(Discriminator, out var type))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(type.Value))
            {
                return false;
            }

            discriminator = type.Value;

            return true;
        }
    }
}
