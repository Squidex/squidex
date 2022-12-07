// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Fluid;
using Fluid.Accessors;
using Fluid.Values;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Templates.Extensions;

public sealed class ContentFluidExtension : IFluidExtension
{
    public void RegisterLanguageExtensions(CustomFluidParser parser, TemplateOptions options)
    {
        options.ValueConverters.Add(source =>
        {
            switch (source)
            {
                case ContentData d:
                    return new ObjectValue(d);
                case ContentFieldData f:
                    return new ObjectValue(f);
                case JsonArray a:
                    return new JsonArrayFluidValue(a, options);
                case JsonObject o:
                    return new ObjectValue(o);
                case JsonValue v:
                    switch (v.Value)
                    {
                        case null:
                            return NilValue.Instance;
                        case bool b:
                            return BooleanValue.Create(b);
                        case double n:
                            return NumberValue.Create((decimal)n);
                        case string s:
                            return StringValue.Create(s);
                        case JsonArray a:
                            return new JsonArrayFluidValue(a, options);
                        case JsonObject o:
                            return new ObjectValue(o);
                    }

                    ThrowHelper.InvalidOperationException();
                    break;
            }

            return null;
        });

        options.MemberAccessStrategy.Register<ContentData>("*", new DelegateAccessor<ContentData, object?>((source, name, context) =>
        {
            return source.GetValueOrDefault(name);
        }));

        options.MemberAccessStrategy.Register<ContentFieldData>("*", new DelegateAccessor<ContentFieldData, object?>((source, name, context) =>
        {
            return source.GetValueOrDefault(name);
        }));

        options.MemberAccessStrategy.Register<JsonObject>("*", new DelegateAccessor<JsonObject, object?>((source, name, context) =>
        {
            return source.GetValueOrDefault(name);
        }));
    }
}
