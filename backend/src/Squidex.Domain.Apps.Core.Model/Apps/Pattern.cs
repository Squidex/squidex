// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record Pattern(string Name, string Regex)
    {
        public string Name { get; } = Guard.NotNullOrEmpty(Name);

        public string Regex { get; } = Guard.NotNullOrEmpty(Regex);

        public string? Message { get; init; }
    }
}
