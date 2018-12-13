// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema.Converters;

namespace Squidex.Areas.Api.Controllers
{
    public class MyJsonInheritanceConverter : JsonInheritanceConverter
    {
        private readonly IDictionary<string, Type> mapping;
        private readonly Type baseType;

        public MyJsonInheritanceConverter(string discriminator, Type baseType)
            : base(baseType, discriminator)
        {
            this.baseType = baseType;
        }

        public MyJsonInheritanceConverter(string discriminator, Type baseType, IDictionary<string, Type> mapping)
            : this(discriminator, baseType)
        {
            this.mapping = mapping;
        }

        public override string GetDiscriminatorValue(Type type)
        {
            var result = type.Name;

            if (baseType != null)
            {
                var baseName = baseType.Name;

                if (result.EndsWith(baseName, StringComparison.CurrentCulture))
                {
                    return result.Substring(0, result.Length - baseName.Length);
                }
            }

            return mapping?.FirstOrDefault(x => x.Value == type).Key ?? result;
        }
    }
}