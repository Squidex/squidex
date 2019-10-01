// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Translations
{
    public sealed class NoopTranslator : ITranslator
    {
        public Task<Translation> Translate(string sourceText, Language targetLanguage, Language? sourceLanguage = null, CancellationToken ct = default)
        {
            var result = new Translation(TranslationResult.NotImplemented);

            return Task.FromResult(result);
        }
    }
}
