// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Translator.State.Old
{
    public class OldTranslatedText
    {
        public SortedDictionary<string, string> Texts { get; set; } = new SortedDictionary<string, string>();

        public SortedSet<TextOrigin> Origins { get; set; } = new SortedSet<TextOrigin>();
    }
}
