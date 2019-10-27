// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Translations
{
    public sealed class Translation
    {
        public TranslationResult Result { get; }

        public string? Text { get; }

        public string? ResultText { get; set; }

        public Translation(TranslationResult result, string? text = null, string? resultText = null)
        {
            Text = text;
            Result = result;
            ResultText = resultText;
        }
    }
}
