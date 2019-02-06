// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class FieldRegistry
    {
        private const string Suffix = "Properties";
        private const string SuffixOld = "FieldProperties";

        public static TypeNameRegistry MapFields(this TypeNameRegistry typeNameRegistry)
        {
            var types = typeof(FieldRegistry).Assembly.GetTypes().Where(x => typeof(FieldProperties).IsAssignableFrom(x) && !x.IsAbstract);

            var addedTypes = new HashSet<Type>();

            foreach (var type in types)
            {
                if (addedTypes.Add(type))
                {
                    typeNameRegistry.Map(type, type.TypeName(false, Suffix));

                    typeNameRegistry.MapObsolete(type, type.TypeName(false, SuffixOld));
                }
            }

            return typeNameRegistry;
        }
    }
}
