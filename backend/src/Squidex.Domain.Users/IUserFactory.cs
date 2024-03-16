// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;

namespace Squidex.Domain.Users
{
    public interface IUserFactory
    {
        IdentityUser Create(string email);

        bool IsId(string id);
    }
}
