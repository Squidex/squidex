// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Accessors;
using Fluid.Values;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class UserFluidExtension : IFluidExtension
{
    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.MemberAccessStrategy.Register<IUser>(new DelegateAccessor<IUser, FluidValue>((source, name, context) =>
        {
            switch (name)
            {
                case "id":
                    return StringValue.Create(source.Id);
                case "email":
                    return StringValue.Create(source.Email);
                case "name":
                    return StringValue.Create(source.Claims.DisplayName());
                default:
                    {
                        var claim = source.Claims.FirstOrDefault(x => string.Equals(name, x.Type, StringComparison.OrdinalIgnoreCase));

                        if (claim != null)
                        {
                            return StringValue.Create(claim.Value);
                        }

                        return NilValue.Instance;
                    }
            }
        }));
    }
}
