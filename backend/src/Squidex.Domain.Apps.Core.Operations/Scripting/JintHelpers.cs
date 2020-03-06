// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Native.Date;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal static class JintHelpers
    {
        public static Engine AddHelpers(this Engine engine)
        {
            engine.SetValue("slugify", new ClrFunctionInstance(engine, "slugify", Slugify));
            engine.SetValue("formatTime", new ClrFunctionInstance(engine, "formatTime", FormatDate));
            engine.SetValue("formatDate", new ClrFunctionInstance(engine, "formatDate", FormatDate));

            return engine;
        }

        public static Engine AddFormatters(this Engine engine, Dictionary<string, Func<string>>? customFormatters = null)
        {
            if (customFormatters != null)
            {
                foreach (var (key, value) in customFormatters)
                {
                    engine.SetValue(key, Safe(value));
                }
            }

            engine.AddHelpers();

            return engine;
        }

        private static Func<string> Safe(Func<string> func)
        {
            return () =>
            {
                try
                {
                    return func();
                }
                catch
                {
                    return "null";
                }
            };
        }

        private static JsValue Slugify(JsValue thisObject, JsValue[] arguments)
        {
            try
            {
                var stringInput = TypeConverter.ToString(arguments.At(0));
                var single = false;

                if (arguments.Length > 1)
                {
                    single = TypeConverter.ToBoolean(arguments.At(1));
                }

                return stringInput.Slugify(null, single);
            }
            catch
            {
                return JsValue.Undefined;
            }
        }

        private static JsValue FormatDate(JsValue thisObject, JsValue[] arguments)
        {
            try
            {
                var dateValue = ((DateInstance)arguments.At(0)).ToDateTime();
                var dateFormat = TypeConverter.ToString(arguments.At(1));

                return dateValue.ToString(dateFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
                return JsValue.Undefined;
            }
        }
    }
}
