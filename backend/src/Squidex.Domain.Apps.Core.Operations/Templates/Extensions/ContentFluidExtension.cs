// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Values;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Templates.Extensions
{
    public sealed class ContentFluidExtension : IFluidExtension
    {
        public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
        {
            FluidValue.SetTypeMapping<JsonObject>(x => new ObjectValue(x));
            FluidValue.SetTypeMapping<JsonArray>(x => new JsonArrayFluidValue(x));
            FluidValue.SetTypeMapping<JsonString>(x => FluidValue.Create(x.Value));
            FluidValue.SetTypeMapping<JsonBoolean>(x => FluidValue.Create(x.Value));
            FluidValue.SetTypeMapping<JsonNumber>(x => FluidValue.Create(x.Value));
            FluidValue.SetTypeMapping<JsonNull>(_ => FluidValue.Create(null));

            memberAccessStrategy.Register<ContentData, object?>(
                (value, name) => value.GetOrDefault(name));

            memberAccessStrategy.Register<JsonObject, object?>(
                (value, name) => value.GetOrDefault(name));

            memberAccessStrategy.Register<ContentFieldData, object?>(
                (value, name) => value.GetOrDefault(name));
        }
    }
}
