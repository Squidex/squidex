// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas;

public class FieldTypeProvider : ITypeProvider
{
    private const string Suffix = "Properties";
    private const string SuffixOld = "FieldProperties";

    public void Map(TypeRegistry typeRegistry)
    {
        static IEnumerable<Type> FindTypes(Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract);
        }

        var config = typeRegistry[typeof(FieldProperties)];

        foreach (var type in FindTypes(typeof(FieldProperties)))
        {
            config.Add(type, type.TypeName(false, SuffixOld));
            config.Add(type, type.TypeName(false, Suffix));
        }
    }
}
