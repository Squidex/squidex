// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Jint;
using Jint.Native;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class StringWordsJintExtension : IJintExtension
    {
        private readonly Func<string, JsValue> wordCount = text =>
        {
            try
            {
                return text.WordCount();
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        private readonly Func<string, JsValue> characterCount = text =>
        {
            try
            {
                return text.CharacterCount();
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        public void Extend(Engine engine)
        {
            engine.SetValue("wordCount", wordCount);

            engine.SetValue("characterCount", characterCount);
        }
    }
}
