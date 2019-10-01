﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptContext
    {
        public ClaimsPrincipal User { get; set; }

        public Guid ContentId { get; set; }

        public NamedContentData? Data { get; set; }

        public NamedContentData DataOld { get; set; }

        public Status Status { get; set; }

        public Status StatusOld { get; set; }

        public string Operation { get; set; }
    }
}
