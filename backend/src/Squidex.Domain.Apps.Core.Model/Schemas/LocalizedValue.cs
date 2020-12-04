// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection.Equality;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class LocalizedValue<T> : Dictionary<string, T>, IEquatable<Dictionary<string, T>>
    {
        public override bool Equals(object? obj)
        {
            return Equals(obj as LocalizedValue<T>);
        }

        public bool Equals(Dictionary<string, T>? other)
        {
            return this.EqualsDictionary(other, EqualityComparer<string>.Default, DeepEqualityComparer<T>.Default);
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode(EqualityComparer<string>.Default, DeepEqualityComparer<T>.Default);
        }

        public T GetLocalizedValue()
        {
            if (TryGetValue(CultureInfo.CurrentUICulture.ToString(), out var current))
            {
                return current;
            }

            if (TryGetValue("en", out var english))
            {
                return english;
            }

            var key = Keys.FirstOrDefault();

            return this.GetValueOrDefault(key);
        }
    }
}
