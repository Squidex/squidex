// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAdministratorAttribute : ApiAuthorizeAttribute
    {
        public MustBeAdministratorAttribute()
        {
            Roles = SquidexRoles.Administrator;
        }
    }
}
