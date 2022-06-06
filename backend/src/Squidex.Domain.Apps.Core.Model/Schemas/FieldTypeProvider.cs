﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public class FieldTypeProvider : ITypeProvider
    {
        private const string Suffix = "Properties";
        private const string SuffixOld = "FieldProperties";

        public void Map(TypeNameRegistry typeNameRegistry)
        {
            var types = typeof(FieldTypeProvider).Assembly.GetTypes().Where(x => typeof(FieldProperties).IsAssignableFrom(x) && !x.IsAbstract);

            var addedTypes = new HashSet<Type>();

            foreach (var type in types)
            {
                if (addedTypes.Add(type))
                {
                    typeNameRegistry.Map(type, type.TypeName(false, Suffix));
                    typeNameRegistry.MapObsolete(type, type.TypeName(false, SuffixOld));
                }
            }
        }
    }
}
