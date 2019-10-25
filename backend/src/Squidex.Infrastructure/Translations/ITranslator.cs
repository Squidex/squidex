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
    public interface ITranslator
    {
        Task<Translation> Translate(string sourceText, Language targetLanguage, Language? sourceLanguage = null, CancellationToken ct = default);
    }
}
