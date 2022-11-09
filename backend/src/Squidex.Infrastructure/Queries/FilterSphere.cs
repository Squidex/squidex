// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Queries;

public sealed record FilterSphere(double Longitude, double Latitude, double Radius)
{
    public override string ToString()
    {
        return $"Radius({Longitude}, {Latitude}, {Radius})";
    }
}
