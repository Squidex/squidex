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
        bool IsLocked { get; }

        string Id { get; }

        string Email { get; }

        string NormalizedEmail { get; }

        IReadOnlyList<Claim> Claims { get; }

        IReadOnlyList<ExternalLogin> Logins { get; }

        void UpdateEmail(string email);

        void AddClaim(Claim claim);

        void SetClaim(string type, string value);
    }
}
