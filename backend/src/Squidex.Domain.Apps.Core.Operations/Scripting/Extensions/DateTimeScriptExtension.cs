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
        private delegate JsValue FormatDateDelegate(DateTime date, string format);

        public void Extend(Engine engine)
        {
            engine.SetValue("formatTime", new FormatDateDelegate(FormatDate));
            engine.SetValue("formatDate", new FormatDateDelegate(FormatDate));
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
