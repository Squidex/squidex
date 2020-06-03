// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Fluid;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class UserFluidExtension : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            memberAccessStrategy.Register<IUser, object?>((value, name) =>
            {
                if (string.Equals(name, "id", StringComparison.OrdinalIgnoreCase))
                {
                    return value.Id;
                }

                if (string.Equals(name, "email", StringComparison.OrdinalIgnoreCase))
                {
                    return value.Email;
                }

                if (string.Equals(name, "name", StringComparison.OrdinalIgnoreCase))
                {
                    return value.DisplayName();
                }

                return value.Claims.FirstOrDefault(x => string.Equals(name, x.Type, StringComparison.OrdinalIgnoreCase))?.Value;
            });
        }
    }
}
