// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Translations
{
    public sealed class DeepLTranslator : ITranslator
    {
        private const string Url = "https://api.deepl.com/v2/translate";
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string authKey;
        private readonly IJsonSerializer jsonSerializer;

        private sealed class Response
        {
            public ResponseTranslation[] Translations { get; set; }
        }

        private sealed class ResponseTranslation
        {
            public string Text { get; set; }
        }

        public DeepLTranslator(string authKey, IJsonSerializer jsonSerializer)
        {
            Guard.NotNull(authKey, nameof(authKey));
            Guard.NotNull(jsonSerializer, nameof(jsonSerializer));

            this.authKey = authKey;

            this.jsonSerializer = jsonSerializer;
        }

        public async Task<Translation> Translate(string sourceText, Language targetLanguage, Language sourceLanguage = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sourceText) || targetLanguage == null)
            {
                return new Translation(TranslationResult.NotTranslated, sourceText);
            }

            var parameters = new Dictionary<string, string>
            {
                ["auth_key"] = authKey,
                ["text"] = sourceText,
                ["target_lang"] = GetLanguageCode(targetLanguage)
            };

            if (sourceLanguage != null)
            {
                parameters["source_lang"] = GetLanguageCode(sourceLanguage);
            }

            var response = await httpClient.PostAsync(Url, new FormUrlEncodedContent(parameters), ct);
            var responseString = await response.Content.ReadAsStringAsync();

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

        private string GetLanguageCode(Language language)
        {
            return language.Iso2Code.Substring(0, 2).ToUpperInvariant();
        }
    }
}
