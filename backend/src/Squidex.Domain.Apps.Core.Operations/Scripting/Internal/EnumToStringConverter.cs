// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

internal sealed class EnumToStringConverter : IObjectConverter
{
    public static readonly EnumToStringConverter Instance = new EnumToStringConverter();

    private EnumToStringConverter()
    {
    }

    public bool TryConvert(Engine engine, object value, [MaybeNullWhen(false)] out JsValue result)
    {
        if (value is Enum)
        {
            result = value.ToString();
            return true;
        }

        result = JsValue.Null;
        return false;
    }
}
