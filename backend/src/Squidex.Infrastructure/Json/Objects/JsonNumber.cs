// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonNumber : JsonScalar<double>
    {
        public override JsonValueType Type
        {
            get => JsonValueType.Number;
        }

        internal JsonNumber(double value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
