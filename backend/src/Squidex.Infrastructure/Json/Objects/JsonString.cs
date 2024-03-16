// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonString : JsonScalar<string>
    {
        public override JsonValueType Type
        {
            get => JsonValueType.String;
        }

        internal JsonString(string value)
            : base(value)
        {
        }

        public override string ToJsonString()
        {
            return $"\"{Value}\"";
        }
    }
}
