// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.Json;

public sealed class ClaimsPrincipalSurrogate : List<ClaimsIdentitySurrogate>, ISurrogate<ClaimsPrincipal>
{
    public void FromSource(ClaimsPrincipal source)
    {
        foreach (var identity in source.Identities)
        {
            var surrogate = new ClaimsIdentitySurrogate();

            surrogate.FromSource(identity);

            Add(surrogate);
        }
    }

    public ClaimsPrincipal ToSource()
    {
        return new ClaimsPrincipal(this.Select(x => x.ToSource()));
    }
}

public sealed class ClaimsIdentitySurrogate : ISurrogate<ClaimsIdentity>
{
    public string? AuthenticationType { get; set; }

    public ClaimSurrogate[] Claims { get; set; }

    public void FromSource(ClaimsIdentity source)
    {
        AuthenticationType = source.AuthenticationType;

        Claims = source.Claims.Select(claim =>
        {
            var surrogate = new ClaimSurrogate();

            surrogate.FromSource(claim);

            return surrogate;
        }).ToArray();
    }

    public ClaimsIdentity ToSource()
    {
        return new ClaimsIdentity(Claims.Select(x => x.ToSource()), AuthenticationType);
    }
}

public sealed class ClaimSurrogate : ISurrogate<Claim>
{
    public string Type { get; set; }

    public string Value { get; set; }

    public void FromSource(Claim source)
    {
        Type = source.Type;

        Value = source.Value;
    }

    public Claim ToSource()
    {
        return new Claim(Type, Value);
    }
}
