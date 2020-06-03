// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class UserFluidExtension : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            FluidValue.SetTypeMapping<IUser>(x => new UserFluidValue(x));
        }
    }
}
