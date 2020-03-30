// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Jint;
using Jint.Native;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class StringScriptExtension : IScriptExtension
    {
        private delegate JsValue StringSlugifyDelegate(string text, bool single = false);
        private readonly StringSlugifyDelegate slugify;
        private readonly Func<string, JsValue> toCamelCase;
        private readonly Func<string, JsValue> toPascalCase;

        public StringScriptExtension()
        {
            slugify = new StringSlugifyDelegate(Slugify);

            toCamelCase = new Func<string, JsValue>(ToCamelCase);
            toPascalCase = new Func<string, JsValue>(ToPascalCase);
        }

        public void Extend(Engine engine)
        {
            engine.SetValue("slugify", slugify);

            engine.SetValue("toCamelCase", toCamelCase);
            engine.SetValue("toPascalCase", toPascalCase);
        }

        private static JsValue Slugify(string text, bool single = false)
        {
            try
            {
                return text.Slugify(null, single);
            }
            catch
            {
                return JsValue.Undefined;
            }
        }

        private static JsValue ToCamelCase(string text)
        {
            try
            {
                return text.ToCamelCase();
            }
            catch
            {
                return JsValue.Undefined;
            }
        }

        private static JsValue ToPascalCase(string text)
        {
            try
            {
                return text.ToPascalCase();
            }
            catch
            {
                return JsValue.Undefined;
            }
        }
    }
}
