// ==========================================================================
//  J.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Orleans
{
    public struct J<T> : IJsonValue
    {
        private readonly T value;
        private readonly bool isImmutable;

        public T Value
        {
            get { return value; }
        }

        bool IJsonValue.IsImmutable
        {
            get { return isImmutable; }
        }

        object IJsonValue.Value
        {
            get { return Value; }
        }

        [JsonConstructor]
        public J(T value, bool isImmutable = false)
        {
            this.value = value;

            this.isImmutable = isImmutable;
        }

        public static implicit operator T(J<T> value)
        {
            return value.Value;
        }

        public static implicit operator J<T>(T d)
        {
            return new J<T>(d);
        }

        public static Task<J<T>> AsTask(T value)
        {
            return Task.FromResult<J<T>>(value);
        }
    }
}
