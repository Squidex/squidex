// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class UserFluidExtension : IFluidExtension
{
    public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
    {
        memberAccessStrategy.Register<IUser, FluidValue>((user, name) =>
        {
            switch (name)
            {
                case "id":
                    return new StringValue(user.Id);
                case "email":
                    return new StringValue(user.Email);
                case "name":
                    return new StringValue(user.Claims.DisplayName());
                default:
                    {
                        var claim = user.Claims.FirstOrDefault(x => string.Equals(name, x.Type, StringComparison.OrdinalIgnoreCase));

                        if (claim != null)
                        {
                            return new StringValue(claim.Value);
                        }

                        return NilValue.Instance;
                    }
            }
        });
    }
}
