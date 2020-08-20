// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Translator.State.Old
{
    public class OldTranslationState
    {
        public SortedDictionary<string, OldTranslatedText> Texts { get; set;  } = new SortedDictionary<string, OldTranslatedText>();

        public HashSet<string> Ignores { get; set; } = new HashSet<string>();

        public SortedDictionary<string, SortedSet<string>> Todos { get; set; } = new SortedDictionary<string, SortedSet<string>>();
    }
}
