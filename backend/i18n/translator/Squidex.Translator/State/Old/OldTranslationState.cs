// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Translator.State.Old;

public class OldTranslationState
{
    public SortedDictionary<string, OldTranslatedText> Texts { get; set;  } = [];

    public HashSet<string> Ignores { get; set; } = [];

    public SortedDictionary<string, SortedSet<string>> Todos { get; set; } = [];
}
