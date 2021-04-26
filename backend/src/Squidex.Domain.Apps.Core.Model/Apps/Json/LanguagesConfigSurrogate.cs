// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class LanguagesConfigSurrogate : ISurrogate<LanguagesConfig>
    {
        public Dictionary<string, LanguageConfigSurrogate> Languages { get; set; }

        public string Master { get; set; }

        public void FromSource(LanguagesConfig source)
        {
            Languages = source.Languages.ToDictionary(x => x.Key, source =>
            {
                var surrogate = new LanguageConfigSurrogate();

                surrogate.FromSource(source.Value);

                return surrogate;
            });

            Master = source.Master;
        }

        public LanguagesConfig ToSource()
        {
            var languages = Languages.ToDictionary(x => x.Key, x => x.Value.ToSource());

            var master = Master ?? languages.Keys.First();

            return new LanguagesConfig(languages, master);
        }
    }
}
