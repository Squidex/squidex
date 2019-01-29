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
        public static void Setup(TypeNameRegistry typeNameRegistry)
        {
            Guard.NotNull(typeNameRegistry, nameof(typeNameRegistry));

            var types = typeof(FieldRegistry).Assembly.GetTypes().Where(x => x.BaseType == typeof(FieldProperties));

            var supportedFields = new HashSet<Type>();

            foreach (var type in types)
            {
                if (supportedFields.Add(type))
                {
                    typeNameRegistry.Map(type);
                }
            }

            typeNameRegistry.MapObsolete(typeof(ReferencesFieldProperties), "References");
            typeNameRegistry.MapObsolete(typeof(DateTimeFieldProperties), "DateTime");
        }
    }
}
