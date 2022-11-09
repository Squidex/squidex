// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using MongoDB.Bson;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class LuceneQueryVisitor
{
    private readonly Func<string, string>? fieldConverter;

    public LuceneQueryVisitor(Func<string, string>? fieldConverter = null)
    {
        this.fieldConverter = fieldConverter;
    }

    public BsonDocument Visit(Query query)
    {
        switch (query)
        {
            case BooleanQuery booleanQuery:
                return VisitBoolean(booleanQuery);
            case TermQuery termQuery:
                return VisitTerm(termQuery);
            case PhraseQuery phraseQuery:
                return VisitPhrase(phraseQuery);
            case WildcardQuery wildcardQuery:
                return VisitWilcard(wildcardQuery);
            case PrefixQuery prefixQuery:
                return VisitPrefix(prefixQuery);
            case FuzzyQuery fuzzyQuery:
                return VisitFuzzy(fuzzyQuery);
            case NumericRangeQuery<float> rangeQuery:
                return VisitNumericRange(rangeQuery);
            case NumericRangeQuery<double> rangeQuery:
                return VisitNumericRange(rangeQuery);
            case NumericRangeQuery<int> rangeQuery:
                return VisitNumericRange(rangeQuery);
            case NumericRangeQuery<long> rangeQuery:
                return VisitNumericRange(rangeQuery);
            case TermRangeQuery termRangeQuery:
                return VisitTermRange(termRangeQuery);
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    private BsonDocument VisitTermRange(TermRangeQuery termRangeQuery)
    {
        if (!TryParseValue(termRangeQuery.LowerTerm, out var min) ||
            !TryParseValue(termRangeQuery.UpperTerm, out var max))
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        var minField = termRangeQuery.IncludesLower ? "gte" : "gt";
        var maxField = termRangeQuery.IncludesUpper ? "lte" : "lt";

        var doc = new BsonDocument
        {
            ["path"] = GetPath(termRangeQuery.Field),
            [minField] = BsonValue.Create(min),
            [maxField] = BsonValue.Create(max)
        };

        ApplyBoost(termRangeQuery, doc);

        return new BsonDocument
        {
            ["range"] = doc
        };
    }

    private BsonDocument VisitNumericRange<T>(NumericRangeQuery<T> numericRangeQuery) where T : struct, IComparable<T>
    {
        var minField = numericRangeQuery.IncludesMin ? "gte" : "gt";
        var maxField = numericRangeQuery.IncludesMin ? "lte" : "lt";

        var doc = new BsonDocument
        {
            ["path"] = GetPath(numericRangeQuery.Field),
            [minField] = BsonValue.Create(numericRangeQuery.Min),
            [maxField] = BsonValue.Create(numericRangeQuery.Max)
        };

        ApplyBoost(numericRangeQuery, doc);

        return new BsonDocument
        {
            ["range"] = doc
        };
    }

    private BsonDocument VisitFuzzy(FuzzyQuery fuzzyQuery)
    {
        var doc = CreateDefaultDoc(fuzzyQuery, fuzzyQuery.Term);

        if (fuzzyQuery.MaxEdits > 0)
        {
            var fuzzy = new BsonDocument
            {
                ["maxEdits"] = fuzzyQuery.MaxEdits
            };

            if (fuzzyQuery.PrefixLength > 0)
            {
                fuzzy["prefixLength"] = fuzzyQuery.PrefixLength;
            }

            doc["fuzzy"] = fuzzy;
        }

        return new BsonDocument
        {
            ["text"] = doc
        };
    }

    private BsonDocument VisitPrefix(PrefixQuery prefixQuery)
    {
        var doc = CreateDefaultDoc(prefixQuery, new Term(prefixQuery.Prefix.Field, prefixQuery.Prefix.Text + "*"));

        return new BsonDocument
        {
            ["wildcard"] = doc
        };
    }

    private BsonDocument VisitWilcard(WildcardQuery wildcardQuery)
    {
        var doc = CreateDefaultDoc(wildcardQuery, wildcardQuery.Term);

        return new BsonDocument
        {
            ["wildcard"] = doc
        };
    }

    private BsonDocument VisitPhrase(PhraseQuery phraseQuery)
    {
        var terms = phraseQuery.GetTerms();

        var doc = new BsonDocument
        {
            ["path"] = GetPath(terms[0].Field)
        };

        if (terms.Length == 1)
        {
            doc["query"] = terms[0].Text;
        }
        else
        {
            doc["query"] = new BsonArray(terms.Select(x => x.Text));
        }

        if (phraseQuery.Slop != 0)
        {
            doc["slop"] = phraseQuery.Slop;
        }

        ApplyBoost(phraseQuery, doc);

        return new BsonDocument
        {
            ["phrase"] = doc
        };
    }

    private BsonDocument VisitTerm(TermQuery termQuery)
    {
        var doc = CreateDefaultDoc(termQuery, termQuery.Term);

        return new BsonDocument
        {
            ["text"] = doc
        };
    }

    private BsonDocument VisitBoolean(BooleanQuery booleanQuery)
    {
        var doc = new BsonDocument();

        BsonArray? musts = null;
        BsonArray? mustNots = null;
        BsonArray? shoulds = null;

        foreach (var clause in booleanQuery.Clauses)
        {
            var converted = Visit(clause.Query);

            switch (clause.Occur)
            {
                case Occur.MUST:
                    musts ??= new BsonArray();
                    musts.Add(converted);
                    break;
                case Occur.SHOULD:
                    shoulds ??= new BsonArray();
                    shoulds.Add(converted);
                    break;
                case Occur.MUST_NOT:
                    mustNots ??= new BsonArray();
                    mustNots.Add(converted);
                    break;
            }
        }

        if (musts != null)
        {
            doc.Add("must", musts);
        }

        if (mustNots != null)
        {
            doc.Add("mustNot", mustNots);
        }

        if (shoulds != null)
        {
            doc.Add("should", shoulds);
        }

        if (booleanQuery.MinimumNumberShouldMatch > 0)
        {
            doc["minimumShouldMatch"] = booleanQuery.MinimumNumberShouldMatch;
        }

        return new BsonDocument
        {
            ["compound"] = doc
        };
    }

    private BsonDocument CreateDefaultDoc(Query query, Term term)
    {
        var doc = new BsonDocument
        {
            ["path"] = GetPath(term.Field),
            ["query"] = term.Text
        };

        ApplyBoost(query, doc);

        return doc;
    }

    private BsonValue GetPath(string field)
    {
        if (field != "*" && fieldConverter != null)
        {
            field = fieldConverter(field);
        }

        if (field.Contains('*', StringComparison.Ordinal))
        {
            return new BsonDocument
            {
                ["wildcard"] = field
            };
        }

        return field;
    }

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
    private static void ApplyBoost(Query query, BsonDocument doc)
    {
        if (query.Boost != 1)
        {
            doc["score"] = new BsonDocument
            {
                ["boost"] = query.Boost
            };
        }
    }

    private static bool TryParseValue(BytesRef bytes, out object result)
    {
        result = null!;

        try
        {
            var text = Encoding.ASCII.GetString(bytes.Bytes, bytes.Offset, bytes.Length);

            if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            {
                return false;
            }

            var integer = (long)number;

            if (number == integer)
            {
                if (integer is <= int.MaxValue and >= int.MinValue)
                {
                    result = (int)integer;
                }
                else
                {
                    result = integer;
                }
            }
            else
            {
                result = number;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
}
