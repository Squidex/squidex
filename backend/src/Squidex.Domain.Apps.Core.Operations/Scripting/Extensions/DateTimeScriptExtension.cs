// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using Jint;
using Jint.Native;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class DateTimeScriptExtension : IScriptExtension
    {
        private readonly Func<DateTime, string, JsValue> formatDate;

        public DateTimeScriptExtension()
        {
            formatDate = new Func<DateTime, string, JsValue>(FormatDate);
        }

        public void Extend(Engine engine)
        {
            engine.SetValue("formatTime", formatDate);
            engine.SetValue("formatDate", formatDate);
        }

        private static JsValue FormatDate(DateTime date, string format)
        {
            try
            {
                return date.ToString(format, CultureInfo.InvariantCulture);
            }
            catch
            {
                return JsValue.Undefined;
            }
        }
    }
}
