// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;

namespace Squidex.Shared.Users
{
    public interface IUser
    {
        bool IsLocked { get; }

        string Id { get; }

        string Email { get; }

        object Identity { get; }

        IReadOnlyList<Claim> Claims { get; }
    }
}
