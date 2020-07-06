// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Jint;
using Jint.Native;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class StringWordsJintExtension : IJintExtension
    {
        private readonly Func<string, JsValue> wordCount = text =>
        {
            try
            {
                var numWords = 0;

                for (int i = 1; i < text.Length; i++)
                {
                    if (char.IsWhiteSpace(text[i - 1]))
                    {
                        var character = text[i];

                        if (char.IsLetterOrDigit(character) || char.IsPunctuation(character))
                        {
                            numWords++;
                        }
                    }
                }

                if (text.Length > 2)
                {
                    numWords++;
                }

                return numWords;
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
                var characterCount = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    if (char.IsLetter(text[i]))
                    {
                        characterCount++;
                    }
                }

                return characterCount;
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
