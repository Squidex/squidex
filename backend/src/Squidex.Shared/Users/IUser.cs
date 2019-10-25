// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;

namespace Squidex.Shared.Users
{
    public interface IUser
    {
        string Id { get; }

        string Email { get; }

        bool IsLocked { get; }

        IReadOnlyList<Claim> Claims { get; }
    }
}
