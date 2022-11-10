// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Reflection;

public class TypeRegistryTests
{
    private readonly TypeRegistry sut = new TypeRegistry();

    public abstract class Base
    {
    }

    public sealed class DerivedBase : Base
    {
    }

    public sealed class DerivedCustom : Base
    {
    }

    [TypeName("my")]
    public sealed class DerivedAttribute : Base
    {
    }

    [EventType(nameof(DerivedEvent), 2)]
    public sealed class DerivedEvent : Base
    {
    }

    [Fact]
    public void Should_call_provider_from_constructor()
    {
        var provider = A.Fake<ITypeProvider>();

        var registry = new TypeRegistry(Enumerable.Repeat(provider, 1));

        A.CallTo(() => provider.Map(registry))
            .MustHaveHappened();
    }

    [Fact]
    public void Should_call_provider()
    {
        var provider = A.Fake<ITypeProvider>();

        sut.Map(provider);

        A.CallTo(() => provider.Map(sut))
            .MustHaveHappened();
    }

    [Fact]
    public void Should_register_and_resolve_name()
    {
        sut.Add<Base, DerivedAttribute>("My");

        sut[typeof(Base)].TryGetName(typeof(DerivedAttribute), out var name);

        Assert.Equal("My", name);
    }

    [Fact]
    public void Should_register_and_resolve_type()
    {
        sut.Add<Base>(typeof(DerivedAttribute), "My");

        sut[typeof(Base)].TryGetType("My", out var type);

        Assert.Equal(typeof(DerivedAttribute), type);
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_inherited_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut[typeof(Base)].TryGetType("Derived", out var type);

        Assert.Equal(typeof(DerivedBase), type);
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_custom_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut[typeof(Base)].TryGetType("DerivedCustom", out var type);

        Assert.Equal(typeof(DerivedCustom), type);
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_event_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut[typeof(Base)].TryGetType("DerivedEventV2", out var type);

        Assert.Equal(typeof(DerivedEvent), type);
    }
}
