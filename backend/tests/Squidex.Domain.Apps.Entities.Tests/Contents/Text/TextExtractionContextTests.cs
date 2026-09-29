// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Entities.Contents.Text.Extraction;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class TextExtractionContextTests
{
    private readonly object key = new object();
    private readonly TextExtractionContext sut = new TextExtractionContext
    {
        AppId = NamedId.Of(DomainId.NewGuid(), "my-app"),
        SchemaId = NamedId.Of(DomainId.NewGuid(), "my-schema"),
    };

    [Fact]
    public void Should_create_value_only_once()
    {
        var calls = new List<int>();

        var value1 = sut.GetOrAdd(key, 42, x => Create(calls, x));
        var value2 = sut.GetOrAdd(key, 13, x => Create(calls, x));

        Assert.Equal("42", value1);
        Assert.Equal("42", value2);
        Assert.Equal([42], calls);
    }

    [Fact]
    public void Should_cache_null_value()
    {
        var calls = new List<int>();

        var value1 = sut.GetOrAdd(key, 42, x => CreateNull(calls, x));
        var value2 = sut.GetOrAdd(key, 13, x => CreateNull(calls, x));

        Assert.Null(value1);
        Assert.Null(value2);

        Assert.Equal([42], calls);
    }

    [Fact]
    public void Should_dispose_value_if_removed()
    {
        var disposable = A.Fake<IDisposable>();

        sut.GetOrAdd(key, disposable, x => x);
        sut.Remove(key);

        var value = sut.GetOrAdd(key, 0, x => "new");

        Assert.Equal("new", value);
        A.CallTo(() => disposable.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Should_dispose_values_if_disposed()
    {
        var disposable1 = A.Fake<IDisposable>();
        var disposable2 = A.Fake<IDisposable>();

        sut.GetOrAdd(new object(), disposable1, x => x);
        sut.GetOrAdd(new object(), disposable2, x => x);
        sut.Dispose();

        A.CallTo(() => disposable1.Dispose())
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => disposable2.Dispose())
            .MustHaveHappenedOnceExactly();
    }

    private static string Create(List<int> calls, int value)
    {
        calls.Add(value);

        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string? CreateNull(List<int> calls, int value)
    {
        calls.Add(value);

        return null;
    }
}
