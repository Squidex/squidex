// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Runtime.Interop;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

internal sealed class CustomClrConverter : DefaultTypeConverter
{
    public CustomClrConverter(Engine engine)
        : base(engine)
    {
    }

    public override object? Convert(object? value, Type type, IFormatProvider formatProvider)
    {
        if (type == typeof(JsonValue))
        {
            return JsonValue.Create(value);
        }

        return base.Convert(value, type, formatProvider);
    }

    public override bool TryConvert(object? value, Type type, IFormatProvider formatProvider, [NotNullWhen(true)] out object? converted)
    {
        if (type == typeof(JsonValue))
        {
            try
            {
                converted = JsonValue.Create(value);
                return true;
            }
            catch
            {
                converted = null;
                return false;
            }
        }

        return base.TryConvert(value, type, formatProvider, out converted);
    }
}
