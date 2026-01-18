// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Contents;

public sealed record UserInfoValue(string ApiKey, string Role)
{
    public static JsonValue CreateDefault(string role)
    {
        var apiKey =
            Guid.NewGuid().ToString().ToSha256Base64()
                .Replace("+", string.Empty, StringComparison.Ordinal)
                .Replace("/", string.Empty, StringComparison.Ordinal)
                .TrimEnd('=');

        return JsonValue.Object().Add("apiKey", apiKey).Add("role", role);
    }

    public static UserInfoParseResult TryParse(JsonValue value, out UserInfoValue? userInfo)
    {
        Guard.NotNull(value);

        userInfo = null;

        if (value.Value is JsonObject o)
        {
            if (!o.TryGetValue("apiKey", out var found) || found.Value is not string apiKey || string.IsNullOrWhiteSpace(apiKey))
            {
                return UserInfoParseResult.InvalidApiKey;
            }

            if (!o.TryGetValue("role", out found) || found.Value is not string role || string.IsNullOrWhiteSpace(role))
            {
                return UserInfoParseResult.InvalidRole;
            }

            userInfo = new UserInfoValue(apiKey, role);

            return UserInfoParseResult.Success;
        }

        return UserInfoParseResult.InvalidValue;
    }
}
