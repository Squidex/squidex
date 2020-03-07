// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class StringScriptExtension : IScriptExtension
    {
        private delegate JsValue StringSlugifyDelegate(string text, bool single = false);
        private delegate JsValue StringFormatDelegate(string text);

        public void Extend(Engine engine)
        {
            engine.SetValue("slugify", new StringSlugifyDelegate(Slugify));

            engine.SetValue("toCamelCase", new StringFormatDelegate(ToCamelCase));
            engine.SetValue("toPascalCase", new StringFormatDelegate(ToPascalCase));
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
