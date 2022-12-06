// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;
using TagLib.IFD.Tags;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public static class QueryParser
{
    private static readonly Dictionary<string, CharArraySet> StopWords = new Dictionary<string, CharArraySet>(StringComparer.OrdinalIgnoreCase);

    static QueryParser()
    {
        foreach (var type in typeof(StandardAnalyzer).Assembly.GetTypes())
        {
            if (!typeof(Analyzer).IsAssignableFrom(type))
            {
                continue;
            }

            var language = type.Namespace!.Split('.')[^1];

            if (language.Length != 2)
            {
                continue;
            }

            try
            {
                var stopWordMethod =
                    type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(x => x.PropertyType == typeof(CharArraySet));

                if (stopWordMethod != null)
                {
                    var stopWords = (CharArraySet)stopWordMethod.GetValue(null)!;

                    StopWords[language] = stopWords;
                }
            }
            catch (MissingMethodException)
            {
                continue;
            }
        }
    }

    public static string ParseQuery(string query)
    {
        var stopWords = StopWords["en"];

        var separator = query.IndexOf(':', StringComparison.OrdinalIgnoreCase);

        if (separator > 0)
        {
            var languageCode = query[..separator];

            if (string.Equals(languageCode, "none", StringComparison.OrdinalIgnoreCase))
            {
                query = query[(separator + 1)..];

                stopWords = null;
            }
            else if (Language.TryGetLanguage(languageCode, out var language))
            {
                query = query[(separator + 1)..];

                stopWords = StopWords.GetOrAddDefault(language.Iso2Code[..2]) ?? StopWords["en"];
            }
        }

        var tokenizer = new StandardTokenizer(LuceneVersion.LUCENE_48, new StringReader(query));
        var tokenStream = (TokenStream)new StandardFilter(LuceneVersion.LUCENE_48, tokenizer);

        // Stop words are case sensitive, therefore we have to lowercase it first.
        tokenStream = new LowerCaseFilter(LuceneVersion.LUCENE_48, tokenStream);

        if (stopWords != null)
        {
            tokenStream = new StopFilter(LuceneVersion.LUCENE_48, tokenStream, stopWords);
        }

        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            // Actually not idea what this is doing, but it seems to work.
            var attribute = tokenStream.AddAttribute<ICharTermAttribute>();

            tokenStream.Reset();

            using (tokenStream)
            {
                while (tokenStream.IncrementToken())
                {
                    var text = attribute.ToString();

                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(text);
                }
            }

            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }
}
