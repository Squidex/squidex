﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;

#pragma warning disable RECS0014 // If all fields, properties and methods members are static, the class can be made static.

namespace Squidex.Web
{
    public sealed class SquidexWeb
    {
        public static readonly Assembly Assembly = typeof(SquidexWeb).Assembly;
    }
}
