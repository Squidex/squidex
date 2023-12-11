// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;

namespace Squidex.Shared.Identity;

public static partial class SquidexClaimsExtensions
{
    private const string ClientPrefix = "client_";

    private static readonly Regex RegexKeyValueAppClaim = BuildKeyValueAppClaimRegex();
    private static readonly Regex RegexKeyValueClaim = BuildKeyValueClaimRegex();

    public static PermissionSet Permissions(this IEnumerable<Claim> user)
    {
        var permissions = user.GetClaims(SquidexClaimTypes.Permissions).Select(x => x.Value);

        return new PermissionSet(permissions);
    }

    public static bool IsHidden(this IEnumerable<Claim> user)
    {
        return user.HasClaimValue(SquidexClaimTypes.Hidden, "true");
    }

    public static bool HasConsent(this IEnumerable<Claim> user)
    {
        return user.HasClaimValue(SquidexClaimTypes.Consent, "true");
    }

    public static bool HasConsentForEmails(this IEnumerable<Claim> user)
    {
        return user.HasClaimValue(SquidexClaimTypes.ConsentForEmails, "true");
    }

    public static bool HasDisplayName(this IEnumerable<Claim> user)
    {
        return user.HasClaim(SquidexClaimTypes.DisplayName);
    }

    public static bool HasPictureUrl(this IEnumerable<Claim> user)
    {
        return user.HasClaim(SquidexClaimTypes.PictureUrl);
    }

    public static bool IsPictureUrlStored(this IEnumerable<Claim> user)
    {
        return user.HasClaimValue(SquidexClaimTypes.PictureUrl, SquidexClaimTypes.PictureUrlStore);
    }

    public static string? ClientSecret(this IEnumerable<Claim> user)
    {
        return user.GetClaimValue(SquidexClaimTypes.ClientSecret);
    }

    public static string? PictureUrl(this IEnumerable<Claim> user)
    {
        return user.GetClaimValue(SquidexClaimTypes.PictureUrl);
    }

    public static string? DisplayName(this IEnumerable<Claim> user)
    {
        return user.GetClaimValue(SquidexClaimTypes.DisplayName);
    }

    public static string? Answer(this IEnumerable<Claim> user, string name)
    {
        var prefix = $"{name}=";

        foreach (var claim in user)
        {
            if (claim.Type == SquidexClaimTypes.Answer && claim.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return claim.Value[prefix.Length..];
            }
        }

        return null;
    }

    public static int GetTotalApps(this IEnumerable<Claim> user)
    {
        var value = user.GetClaimValue(SquidexClaimTypes.TotalApps);

        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result);

        return result;
    }

    public static bool HasClaim(this IEnumerable<Claim> user, string type)
    {
        return user.GetClaims(type).Any();
    }

    public static bool HasClaimValue(this IEnumerable<Claim> user, string type, string value)
    {
        return user.GetClaims(type).Any(x => string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Claim> GetSquidexClaims(this IEnumerable<Claim> user)
    {
        const string prefix = "urn:squidex:";

        foreach (var claim in user)
        {
            var type = GetType(claim);

            if (type.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                yield return claim;
            }
        }
    }

    public static IEnumerable<(string Name, string Value)> GetCustomProperties(this IEnumerable<Claim> user)
    {
        var prefix = $"{SquidexClaimTypes.Custom}:";

        foreach (var claim in user)
        {
            var type = GetType(claim);

            if (type.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = type[prefix.Length..].ToString();

                yield return (name.Trim(), claim.Value.Trim());
            }
            else if (type.Equals(SquidexClaimTypes.Custom, StringComparison.OrdinalIgnoreCase))
            {
                var match = RegexKeyValueClaim.Match(claim.Value);

                if (match.Success)
                {
                    yield return (match.Groups["Key"].Value.Trim(), match.Groups["Value"].Value.Trim());
                }
            }
        }
    }

    public static IEnumerable<(string Name, JsonValue Value)> GetUIProperties(this IEnumerable<Claim> user, string app)
    {
        var prefix = $"{SquidexClaimTypes.UIProperty}:{app}:";

        static JsonValue Parse(string value)
        {
            value = value.Trim();

            try
            {
                var root = JsonDocument.Parse(value).RootElement;

                return JsonValue.Create(root);
            }
            catch
            {
                return JsonValue.Create(value);
            }
        }

        foreach (var claim in user)
        {
            var type = GetType(claim);

            if (type.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var name = type[prefix.Length..].ToString();

                yield return (name.Trim(), Parse(claim.Value));
            }
            else if (type.Equals(SquidexClaimTypes.UIProperty, StringComparison.OrdinalIgnoreCase))
            {
                if (!claim.Value.StartsWith(app, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var match = RegexKeyValueAppClaim.Match(claim.Value);

                if (match.Success)
                {
                    yield return (match.Groups["Key"].Value.Trim(), Parse(match.Groups["Value"].Value));
                }
            }
        }
    }

    public static string? PictureNormalizedUrl(this IEnumerable<Claim> user)
    {
        var url = user.FirstOrDefault(x => x.Type == SquidexClaimTypes.PictureUrl)?.Value;

        if (Uri.IsWellFormedUriString(url, UriKind.Absolute) && url.Contains("gravatar", StringComparison.Ordinal))
        {
            if (url.Contains('?', StringComparison.Ordinal))
            {
                url += "&d=404";
            }
            else
            {
                url += "?d=404";
            }
        }

        return url;
    }

    private static string? GetClaimValue(this IEnumerable<Claim> user, string type)
    {
        return user.GetClaims(type).FirstOrDefault()?.Value;
    }

    private static IEnumerable<Claim> GetClaims(this IEnumerable<Claim> user, string request)
    {
        foreach (var claim in user)
        {
            var type = GetType(claim);

            if (type.Equals(request, StringComparison.OrdinalIgnoreCase))
            {
                yield return claim;
            }
        }
    }

    private static ReadOnlySpan<char> GetType(Claim claim)
    {
        var type = claim.Type.AsSpan();

        if (type.StartsWith(ClientPrefix, StringComparison.OrdinalIgnoreCase))
        {
            type = type[ClientPrefix.Length..];
        }

        return type;
    }

    [GeneratedRegex("(?<App>[\\S]+),(?<Key>[^=]+)=(?<Value>.+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex BuildKeyValueAppClaimRegex();

    [GeneratedRegex("(?<Key>[^=]+)=(?<Value>.+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex BuildKeyValueClaimRegex();
}
