// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

namespace Squidex.Infrastructure.Reflection;

public sealed class AssemblyTypeProvider<T> : ITypeProvider where T : class
{
    private readonly Assembly assembly;

    public string? DiscriminatorProperty { get; }

    public AssemblyTypeProvider(string? discriminatorProperty = null)
        : this(typeof(T).Assembly, discriminatorProperty)
    {
    }

    public AssemblyTypeProvider(Assembly assembly, string? discriminatorProperty = null)
    {
        Guard.NotNull(assembly);

        this.assembly = assembly;

        DiscriminatorProperty = discriminatorProperty;
    }

    public void Map(TypeRegistry typeRegistry)
    {
        var baseType = typeof(T);

        foreach (var derivedType in assembly.GetTypes())
        {
            if (derivedType.IsAssignableTo(baseType) && !derivedType.IsAbstract)
            {
                var typeName = derivedType.GetCustomAttribute<TypeNameAttribute>()?.TypeName;

                if (string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = derivedType.TypeName(false, baseType.Name);
                }

                typeRegistry.Add<T>(derivedType, typeName);
            }
        }

        typeRegistry.Discriminator<T>(DiscriminatorProperty);
    }
}
