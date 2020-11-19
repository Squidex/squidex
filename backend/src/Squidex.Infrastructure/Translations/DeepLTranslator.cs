// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Translations
{
    [ExcludeFromCodeCoverage]
    public sealed class DeepLTranslator : ITranslator
    {
        private const string Url = "https://api.deepl.com/v2/translate";
        private readonly HttpClient httpClient = new HttpClient();
        private readonly DeepLTranslatorOptions deeplOptions;
        private readonly IJsonSerializer jsonSerializer;

        private sealed class Response
        {
            public ResponseTranslation[] Translations { get; set; }
        }

        private sealed class ResponseTranslation
        {
            public string Text { get; set; }
        }

        public DeepLTranslator(IOptions<DeepLTranslatorOptions> deeplOptions, IJsonSerializer jsonSerializer)
        {
            Guard.NotNull(deeplOptions, nameof(deeplOptions));
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));

            this.deeplOptions = deeplOptions.Value;

            this.jsonSerializer = jsonSerializer;
        }

        public async Task<Translation> Translate(string sourceText, Language targetLanguage, Language? sourceLanguage = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sourceText) || targetLanguage == null)
            {
                return new Translation(TranslationResult.NotTranslated, sourceText);
            }

            if (string.IsNullOrWhiteSpace(deeplOptions.AuthKey))
            {
                return new Translation(TranslationResult.NotImplemented);
            }

            var parameters = new Dictionary<string, string>
            {
                ["auth_key"] = deeplOptions.AuthKey,
                ["text"] = sourceText,
                ["target_lang"] = GetLanguageCode(targetLanguage)
            };

            if (sourceLanguage != null)
            {
                parameters["source_lang"] = GetLanguageCode(sourceLanguage);
            }

            var body = new FormUrlEncodedContent(parameters!);

            using (var response = await httpClient.PostAsync(Url, body, ct))
            {
                var responseString = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = jsonSerializer.Deserialize<Response>(responseString);

                    if (result?.Translations?.Length == 1)
                    {
                        return new Translation(TranslationResult.Translated, result.Translations[0].Text);
                    }
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new Translation(TranslationResult.LanguageNotSupported, resultText: responseString);
                }

                return new Translation(TranslationResult.Failed, resultText: responseString);
            }
        }

        private static string GetLanguageCode(Language language)
        {
            return language.Iso2Code.Substring(0, 2).ToUpperInvariant();
        }
    }
}
