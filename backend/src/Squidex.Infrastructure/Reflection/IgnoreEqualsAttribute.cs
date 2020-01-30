// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Reflection
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreEqualsAttribute : Attribute
    {
    }
}
