// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Translator.State
{
    public sealed class TranslatedTexts : SortedDictionary<string, string>
    {
        public TranslatedTexts()
        {
        }

        public TranslatedTexts(TranslatedTexts source)
            : base(source)
        {
        }
    }
}
