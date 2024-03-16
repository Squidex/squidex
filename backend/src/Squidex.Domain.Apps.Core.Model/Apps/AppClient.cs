// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record AppClient(string Name, string Secret)
    {
        public string Role { get; init; } = "Editor";

        public long ApiCallsLimit { get; init; }

        public long ApiTrafficLimit { get; init; }

        public bool AllowAnonymous { get; init; }
    }
}
