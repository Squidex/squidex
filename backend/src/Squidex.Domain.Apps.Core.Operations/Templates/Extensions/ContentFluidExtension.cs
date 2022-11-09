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

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class ContentFluidExtension : IFluidExtension
{
    public void RegisterGlobalTypes(IMemberAccessStrategy memberAccessStrategy)
    {
        FluidValue.SetTypeMapping<ContentData>(x => new ObjectValue(x));
        FluidValue.SetTypeMapping<ContentFieldData>(x => new ObjectValue(x));
        FluidValue.SetTypeMapping<JsonObject>(x => new ObjectValue(x));
        FluidValue.SetTypeMapping<JsonArray>(x => new JsonArrayFluidValue(x));

        FluidValue.SetTypeMapping<JsonValue>(source =>
        {
            switch (source.Value)
            {
                case null:
                    return FluidValue.Create(null);
                case bool b:
                    return FluidValue.Create(b);
                case double n:
                    return FluidValue.Create(n);
                case string s:
                    return FluidValue.Create(s);
                case JsonObject o:
                    return new ObjectValue(o);
                case JsonArray a:
                    return new JsonArrayFluidValue(a);
            }

            ThrowHelper.InvalidOperationException();
            return default!;
        });

        memberAccessStrategy.Register<JsonValue, object?>((value, name) =>
        {
            if (value.Value is JsonObject o)
            {
                return o.GetValueOrDefault(name);
            }

            return null;
        });

        memberAccessStrategy.Register<ContentData, object?>(
            (value, name) => value.GetValueOrDefault(name));

        memberAccessStrategy.Register<ContentFieldData, object?>(
            (value, name) => value.GetValueOrDefault(name).Value);

        memberAccessStrategy.Register<JsonObject, object?>(
            (value, name) => value.GetValueOrDefault(name).Value);
    }
}
