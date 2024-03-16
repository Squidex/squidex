// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Json.Objects
{
    public sealed class JsonBoolean : JsonScalar<bool>
    {
        public static readonly JsonBoolean True = new JsonBoolean(true);
        public static readonly JsonBoolean False = new JsonBoolean(false);

        public override JsonValueType Type
        {
            get => JsonValueType.Boolean;
        }

        private JsonBoolean(bool value)
            : base(value)
        {
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}
