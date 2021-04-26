// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq.Expressions;
using MongoDB.Driver;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class FieldDefinitionBuilder<T>
    {
        public static readonly FieldDefinitionBuilder<T> Instance = new FieldDefinitionBuilder<T>();

        private FieldDefinitionBuilder()
        {
        }

        public FieldDefinition<T, TResult> Build<TResult>(Expression<Func<T, TResult>> expression)
        {
            return new ExpressionFieldDefinition<T, TResult>(expression);
        }

        public FieldDefinition<T, TResult> Build<TResult>(string name)
        {
            return new StringFieldDefinition<T, TResult>(name);
        }
    }
}
