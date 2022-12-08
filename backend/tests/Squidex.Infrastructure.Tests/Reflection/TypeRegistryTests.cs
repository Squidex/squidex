// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

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

        sut.TryGetName<Base>(typeof(DerivedAttribute), out var name);

        Assert.Equal("My", name);
        Assert.Equal("My", sut.GetName<Base, DerivedAttribute>());
    }

    [Fact]
    public void Should_register_and_resolve_type()
    {
        sut.Add<Base>(typeof(DerivedAttribute), "My");

        sut.TryGetType<Base>("My", out var type);

        Assert.Equal(typeof(DerivedAttribute), type);
        Assert.Equal(typeof(DerivedAttribute), sut.GetType<Base>("My"));
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_inherited_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut.TryGetType<Base>("Derived", out var type);

        Assert.Equal(typeof(DerivedBase), type);
        Assert.Equal(typeof(DerivedBase), sut.GetType<Base>("Derived"));
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_custom_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut.TryGetType<Base>("DerivedCustom", out var type);

        Assert.Equal(typeof(DerivedCustom), type);
        Assert.Equal(typeof(DerivedCustom), sut.GetType<Base>("DerivedCustom"));
    }

    [Fact]
    public void Should_register_with_provider_from_assembly_and_event_name()
    {
        sut.Map(new AssemblyTypeProvider<Base>());

        sut.TryGetType<Base>("DerivedEventV2", out var type);

        Assert.Equal(typeof(DerivedEvent), type);
        Assert.Equal(typeof(DerivedEvent), sut.GetType<Base>("DerivedEventV2"));
    }

    [Fact]
    public void Should_throw_exception_if_type_name_not_found()
    {
        Assert.Throws<ArgumentException>(() => sut.GetName<Base>(typeof(DerivedCustom)));
    }

    [Fact]
    public void Should_throw_exception_if_type_not_found()
    {
        Assert.Throws<ArgumentException>(() => sut.GetType<Base>("DerivedCustom"));
    }

    [Fact]
    public void Should_register_in_parallel()
    {
        Parallel.ForEach(Enumerable.Range(0, 100), x =>
        {
            sut.Add<Base, DerivedAttribute>("My");
        });
    }
}
