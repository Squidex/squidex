﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class JsonError
    {
        public string Error { get; }

        public JsonError(string error)
        {
            Error = error;
        }
    }
}
