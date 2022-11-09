// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Jint;
using Jint.Native;
using Squidex.Domain.Apps.Core.Properties;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class DateTimeJintExtension : IJintExtension, IScriptDescriptor
{
    private readonly Func<DateTime, string, JsValue> formatDate = (date, format) =>
    {
        try
        {
            return date.ToString(format, CultureInfo.InvariantCulture);
        }
        catch
        {
            return JsValue.Undefined;
        }
    };

    public void Extend(Engine engine)
    {
        engine.SetValue("formatTime", formatDate);
        engine.SetValue("formatDate", formatDate);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        describe(JsonType.Function, "formatDate(data, pattern)",
            Resources.ScriptingFormatDate);

        describe(JsonType.Function, "formatTime(text)",
            Resources.ScriptingFormatTime);
    }
}
