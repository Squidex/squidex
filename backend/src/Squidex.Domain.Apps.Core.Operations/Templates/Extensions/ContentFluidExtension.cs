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
            FluidValue.SetTypeMapping<JsonValue2>(source =>
            {
                switch (source.Type)
                {
                    case JsonValueType.Null:
                        return FluidValue.Create(null);
                    case JsonValueType.Boolean:
                        return FluidValue.Create(source.AsBoolean);
                    case JsonValueType.Number:
                        return FluidValue.Create(source.AsNumber);
                    case JsonValueType.String:
                        return FluidValue.Create(source.AsString);
                    case JsonValueType.Array:
                        return new JsonArrayFluidValue(source.AsArray);
                    case JsonValueType.Object:
                        return new ObjectValue(source.AsObject);
                    default:
                        throw new InvalidOperationException();
                }
            });

            memberAccessStrategy.Register<ContentData, object?>(
                (value, name) => value.GetOrDefault(name));

            memberAccessStrategy.Register<JsonObject, object?>(
                (value, name) => value.GetOrDefault(name));

            memberAccessStrategy.Register<ContentFieldData, object?>(
                (value, name) => value.GetOrDefault(name));
        }
    }
}
