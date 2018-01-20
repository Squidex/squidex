﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.MongoDb
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BsonJsonAttribute : Attribute
    {
    }
}
