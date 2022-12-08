// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class ErrorVisitor : BaseSchemaNodeVisitor
{
    public static readonly ErrorVisitor Instance = new ErrorVisitor();

    internal sealed class ErrorResolver : IFieldResolver
    {
        private readonly IFieldResolver inner;

        public ErrorResolver(IFieldResolver inner)
        {
            this.inner = inner;
        }

        public async ValueTask<object?> ResolveAsync(IResolveFieldContext context)
        {
            try
            {
                return await inner.ResolveAsync(context);
            }
            catch (ValidationException ex)
            {
                throw new ExecutionError(ex.Message);
            }
            catch (DomainException ex)
            {
                throw new ExecutionError(ex.Message);
            }
            catch (Exception ex)
            {
                var logFactory = context.RequestServices!.GetRequiredService<ILoggerFactory>();

                logFactory.CreateLogger("GraphQL").LogError(ex, "Failed to resolve field {field}.", context.FieldDefinition.Name);
                throw;
            }
        }
    }

    internal sealed class ErrorSourceStreamResolver : ISourceStreamResolver
    {
        private readonly ISourceStreamResolver inner;

        public ErrorSourceStreamResolver(ISourceStreamResolver inner)
        {
            this.inner = inner;
        }

        public async ValueTask<IObservable<object?>> ResolveAsync(IResolveFieldContext context)
        {
            try
            {
                return await inner.ResolveAsync(context);
            }
            catch (ValidationException ex)
            {
                throw new ExecutionError(ex.Message);
            }
            catch (DomainException ex)
            {
                throw new ExecutionError(ex.Message);
            }
            catch (Exception ex)
            {
                var logFactory = context.RequestServices!.GetRequiredService<ILoggerFactory>();

                logFactory.CreateLogger("GraphQL").LogError(ex, "Failed to resolve field {field}.", context.FieldDefinition.Name);
                throw;
            }
        }
    }

    private ErrorVisitor()
    {
    }

    public override void VisitObjectFieldDefinition(FieldType field, IObjectGraphType type, ISchema schema)
    {
        if (type.Name.StartsWith("__", StringComparison.Ordinal))
        {
            return;
        }

        if (field.StreamResolver != null)
        {
            if (field.StreamResolver is ErrorSourceStreamResolver)
            {
                return;
            }

            field.StreamResolver = new ErrorSourceStreamResolver(field.StreamResolver);
        }
        else
        {
            if (field.Resolver is ErrorResolver)
            {
                return;
            }

            field.Resolver = new ErrorResolver(field.Resolver ?? NameFieldResolver.Instance);
        }
    }
}
