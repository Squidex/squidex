// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.MongoDb.Text;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public class TokenizerTests
{
    [Fact]
    public void Should_eliminate_stop_words_in_implicit_english()
    {
        var source = "The only thing that matters, is time.";

        var parsed = Tokenizer.Query(source);

        Assert.Equal("only thing matters time", parsed);
    }

    [Fact]
    public void Should_eliminate_stop_words_in_explicit_english()
    {
        var source = "en:The only thing that matters, is time.";

        var parsed = Tokenizer.Query(source);

        Assert.Equal("only thing matters time", parsed);
    }

    [Fact]
    public void Should_eliminate_stop_words_in_explicit_english2()
    {
        var source = "en:when i do this it is pretty slow";

        var parsed = Tokenizer.Query(source);

        Assert.Equal("when i do pretty slow", parsed);
    }

    [Fact]
    public void Should_not_eliminate_stop_words_for_iv_language()
    {
        var source = "iv:The only thing that matters, is time.";

        var parsed = Tokenizer.Query(source);

        Assert.Equal("the only thing that matters is time", parsed);
    }

    [Fact]
    public void Should_eliminate_stop_words_in_explicit_german()
    {
        var source = "de:Nur die Zeit spielt eine Rolle";

        var parsed = Tokenizer.Query(source);

        Assert.Equal("zeit spielt rolle", parsed);
    }
}
