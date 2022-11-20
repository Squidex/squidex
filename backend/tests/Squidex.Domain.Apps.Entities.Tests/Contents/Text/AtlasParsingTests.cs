// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using System.Text.Json;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using MongoDB.Bson;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using LuceneQueryAnalyzer = Lucene.Net.QueryParsers.Classic.QueryParser;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class AtlasParsingTests
{
    private static readonly LuceneQueryVisitor QueryVisitor = new LuceneQueryVisitor();
    private static readonly LuceneQueryAnalyzer QueryParser =
        new LuceneQueryAnalyzer(LuceneVersion.LUCENE_48, "*",
            new StandardAnalyzer(LuceneVersion.LUCENE_48, CharArraySet.EMPTY_SET));
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    [Fact]
    public void Should_parse_term_query()
    {
        var actual = ParseQuery("hello");

        var expected = CreateQuery(new
        {
            text = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = "hello"
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_phrase_query()
    {
        var actual = ParseQuery("\"hello dolly\"");

        var expected = CreateQuery(new
        {
            phrase = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = new[] { "hello", "dolly" }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_compound_phrase_query()
    {
        var actual = ParseQuery("title:\"The Right Way\" AND text:go");

        var expected = CreateQuery(new
        {
            compound = new
            {
                must = new object[]
                {
                    new
                    {
                        phrase = new
                        {
                            path = "title",
                            query = new[]
                            {
                                "the",
                                "right",
                                "way"
                            }
                        }
                    },
                    new
                    {
                        text = new
                        {
                            path = "text",
                            query = "go"
                        }
                    }
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_compound_phrase_query_with_widldcard()
    {
        var actual = ParseQuery("title:\"Do it right\" AND right");

        var expected = CreateQuery(new
        {
            compound = new
            {
                must = new object[]
                {
                    new
                    {
                        phrase = new
                        {
                            path = "title",
                            query = new[]
                            {
                                "do",
                                "it",
                                "right"
                            }
                        }
                    },
                    new
                    {
                        text = new
                        {
                            path = new
                            {
                                wildcard = "*"
                            },
                            query = "right"
                        }
                    }
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_wildcard_query()
    {
        var actual = ParseQuery("te?t");

        var expected = CreateQuery(new
        {
            wildcard = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = "te?t"
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_prefix_query()
    {
        var actual = ParseQuery("test*");

        var expected = CreateQuery(new
        {
            wildcard = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = "test*"
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_fuzzy_query()
    {
        var actual = ParseQuery("roam~");

        var expected = CreateQuery(new
        {
            text = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = "roam",
                fuzzy = new
                {
                    maxEdits = 2
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_fuzzy_query_with_max_edits()
    {
        var actual = ParseQuery("roam~1");

        var expected = CreateQuery(new
        {
            text = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = "roam",
                fuzzy = new
                {
                    maxEdits = 1
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_fuzzy_phrase_query_with_slop()
    {
        var actual = ParseQuery("\"jakarta apache\"~10");

        var expected = CreateQuery(new
        {
            phrase = new
            {
                path = new
                {
                    wildcard = "*"
                },
                query = new[]
                {
                    "jakarta",
                    "apache"
                },
                slop = 10
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_compound_query_with_brackets()
    {
        var actual = ParseQuery("(jakarta OR apache) AND website");

        var expected = CreateQuery(new
        {
            compound = new
            {
                must = new object[]
                {
                    new
                    {
                        compound = new
                        {
                            should = new object[]
                            {
                                new
                                {
                                    text = new
                                    {
                                        path = new
                                        {
                                            wildcard = "*"
                                        },
                                        query = "jakarta"
                                    }
                                },
                                new
                                {
                                    text = new
                                    {
                                        path = new
                                        {
                                            wildcard = "*"
                                        },
                                        query = "apache"
                                    }
                                }
                            }
                        }
                    },
                    new
                    {
                        text = new
                        {
                            path = new
                            {
                                wildcard = "*"
                            },
                            query = "website"
                        }
                    }
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_compound_query_and_optimize()
    {
        var actual = ParseQuery("title:(+return +\"pink panther\")");

        var expected = CreateQuery(new
        {
            compound = new
            {
                must = new object[]
                {
                    new
                    {
                        text = new
                        {
                            path = "title",
                            query = "return"
                        }
                    },
                    new
                    {
                        phrase = new
                        {
                            path = "title",
                            query = new[]
                            {
                                "pink",
                                "panther"
                            }
                        }
                    }
                }
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_range_query()
    {
        var actual = ParseQuery("mod_date:[20020101 TO 20030101]");

        var expected = CreateQuery(new
        {
            range = new
            {
                path = "mod_date",
                gte = 20020101,
                lte = 20030101
            }
        });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_parse_open_range_query()
    {
        var actual = ParseQuery("mod_date:{20020101 TO 20030101}");

        var expected = CreateQuery(new
        {
            range = new
            {
                path = "mod_date",
                gt = 20020101,
                lt = 20030101
            }
        });

        Assert.Equal(expected, actual);
    }

    private static object CreateQuery(object query)
    {
        return JsonSerializer.Serialize(query, JsonSerializerOptions);
    }

    private static object ParseQuery(string query)
    {
        var luceneQuery = QueryParser.Parse(query);

        var rendered = QueryVisitor.Visit(luceneQuery);

        var jsonStream = new MemoryStream();
        var jsonDocument = JsonDocument.Parse(rendered.ToJson());

        var jsonWriter = new Utf8JsonWriter(jsonStream, new JsonWriterOptions { Indented = true });

        jsonDocument.WriteTo(jsonWriter);

        jsonWriter.Flush();

        return Encoding.UTF8.GetString(jsonStream.ToArray());
    }
}
