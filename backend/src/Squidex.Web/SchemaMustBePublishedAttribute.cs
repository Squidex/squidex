// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class SchemaMustBePublishedAttribute : Attribute
    {
    }
}
