// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Domain.Apps.Core.ConvertContent
{
    public sealed class Value
    {
        public static readonly JToken Unset = JValue.CreateUndefined();

        private Value()
        {
        }
    }
}
