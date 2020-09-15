// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class InputFieldExtensions
    {
        public static IGraphType GetInputGraphType(this IField field)
        {
            return field.Accept(InputFieldVisitor.Default);
        }
    }
}
