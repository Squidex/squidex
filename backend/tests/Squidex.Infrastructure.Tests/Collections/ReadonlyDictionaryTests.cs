// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Collections;

public class ReadonlyDictionaryTests
{
    internal sealed class Inherited : ReadonlyDictionary<int, int>
    {
        public Inherited(IDictionary<int, int> inner)
            : base(inner)
        {
        }
    }

    [Fact]
    public void Should_return_empty_instance_for_empty_source()
    {
        var actual = new Dictionary<int, int>().ToReadonlyDictionary();

        Assert.Same(ReadonlyDictionary.Empty<int, int>(), actual);
    }

    [Fact]
    public void Should_return_empty_instance_for_empty_source_and_key_selector()
    {
        var actual = Enumerable.Empty<int>().ToReadonlyDictionary(x => x);

        Assert.Same(ReadonlyDictionary.Empty<int, int>(), actual);
    }

    [Fact]
    public void Should_return_empty_instance_for_empty_source_and_value_selector()
    {
        var actual = Enumerable.Empty<int>().ToReadonlyDictionary(x => x, x => x);

        Assert.Same(ReadonlyDictionary.Empty<int, int>(), actual);
    }

    [Fact]
    public void Should_make_correct_object_equal_comparisons()
    {
        var obj1a = new Dictionary<int, int>
        {
            [1] = 1
        }.ToReadonlyDictionary();

        var obj1b = new Dictionary<int, int>
        {
            [1] = 1
        }.ToReadonlyDictionary();

        var dictionaryOtherValue = new Dictionary<int, int>
        {
            [1] = 2
        }.ToReadonlyDictionary();

        var dictionaryOtherKey = new Dictionary<int, int>
        {
            [2] = 1
        }.ToReadonlyDictionary();

        var dictionaryOtherCount = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 2
        }.ToReadonlyDictionary();

        Assert.Equal(obj1a, obj1b);
        Assert.Equal(obj1a.GetHashCode(), obj1b.GetHashCode());
        Assert.True(obj1a.Equals((object)obj1b));

        Assert.NotEqual(obj1a, dictionaryOtherValue);
        Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherValue.GetHashCode());
        Assert.False(obj1a.Equals((object)dictionaryOtherValue));

        Assert.NotEqual(obj1a, dictionaryOtherKey);
        Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherKey.GetHashCode());
        Assert.False(obj1a.Equals((object)dictionaryOtherKey));

        Assert.NotEqual(obj1a, dictionaryOtherCount);
        Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherCount.GetHashCode());
        Assert.False(obj1a.Equals((object)dictionaryOtherCount));
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut = new Dictionary<int, int>
        {
            [11] = 1,
            [12] = 2,
            [13] = 3
        }.ToReadonlyDictionary();

        var serialized = sut.SerializeAndDeserialize();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_inherited()
    {
        var sut = new Inherited(new Dictionary<int, int>
        {
            [11] = 1,
            [12] = 2,
            [13] = 3
        });

        var serialized = sut.SerializeAndDeserialize();

        Assert.Equal(sut, serialized);
    }
}
