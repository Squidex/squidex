// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public static class Tokenizer
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;
    private static readonly Dictionary<string, CharArraySet> StopWords = new Dictionary<string, CharArraySet>(StringComparer.OrdinalIgnoreCase);

    static Tokenizer()
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

    public static string Query(string query)
    {
        query = query.Trim();

        var textLanguage = Language.EN.Iso2Code;
        var textReader = new StringReader(query);

        if (query.Length > 2 && query[2] == ':')
        {
            textLanguage = query[..2];
            textReader.Read();
            textReader.Read();
        }

        return Word(textReader, textLanguage);
    }

    public static string Terms(string query, string language)
    {
        var stopWords =
            string.Equals(language, InvariantPartitioning.Key, StringComparison.OrdinalIgnoreCase) ?
            null :
            StopWords.GetValueOrDefault(language) ??
            StopWords["en"];

        return Tokenize(new StringReader(query), stopWords);
    }

    private static string Word(TextReader reader, string language)
    {
        var stopWords =
            string.Equals(language, InvariantPartitioning.Key, StringComparison.OrdinalIgnoreCase) ?
            null :
            StopWords.GetValueOrDefault(language) ??
            StopWords["en"];

        return Tokenize(reader, stopWords);
    }

    private static string Tokenize(TextReader reader, CharArraySet? stopWords)
    {
        var tokenizer = new StandardTokenizer(Version, reader);
        var tokenStream = (TokenStream)new StandardFilter(Version, tokenizer);

        // Stop words are case sensitive, therefore we have to lowercase it first.
        tokenStream = new LowerCaseFilter(Version, tokenStream);

        if (stopWords != null)
        {
            tokenStream = new StopFilter(Version, tokenStream, stopWords);
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
