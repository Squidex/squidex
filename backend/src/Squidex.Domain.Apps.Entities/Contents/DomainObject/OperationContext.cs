// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class OperationContext
    {
        private readonly List<ValidationError> errors = new List<ValidationError>();
        private readonly IServiceProvider serviceProvider;

        public ClaimsPrincipal? User { get; init; }

        public RefToken Actor { get; init; }

        public IAppEntity App { get; init; }

        public ISchemaEntity Schema { get; init; }

        public DomainId ContentId { get; init; }

        public ResolvedComponents Components { get; init; }

        public Func<IContentEntity> ContentProvider { get; init; }

        public IContentEntity Content
        {
            get => ContentProvider();
        }

        public Schema SchemaDef
        {
            get => Schema.SchemaDef;
        }

        public OperationContext(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;
        }

        public static async Task<OperationContext> CreateAsync(IServiceProvider services, ContentCommand command, Func<IContentEntity> snapshot)
        {
            var appProvider = services.GetRequiredService<IAppProvider>();

            var (app, schema) = await appProvider.GetAppWithSchemaAsync(command.AppId.Id, command.SchemaId.Id);

            if (app == null)
            {
                throw new DomainObjectNotFoundException(command.AppId.Id.ToString());
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(command.SchemaId.Id.ToString());
            }

            var components = await appProvider.GetComponentsAsync(schema);

            return new OperationContext(services)
            {
                App = app,
                Actor = command.Actor,
                Components = components,
                ContentProvider = snapshot,
                ContentId = command.ContentId,
                Schema = schema,
                User = command.User
            };
        }

        public T Resolve<T>() where T : notnull
        {
            return serviceProvider.GetRequiredService<T>();
        }

        public T? ResolveOptional<T>() where T : class
        {
            return serviceProvider.GetService(typeof(T)) as T;
        }

        public OperationContext AddError(string message, params string[] propertyNames)
        {
            errors.Add(new ValidationError(message, propertyNames));

            return this;
        }

        public OperationContext AddError(ValidationError newError)
        {
            errors.Add(newError);

            return this;
        }

        public OperationContext AddErrors(IEnumerable<ValidationError> newErrors)
        {
            errors.AddRange(newErrors);

            return this;
        }

        public void ThrowOnErrors()
        {
            if (errors.Count > 0)
            {
                throw new ValidationException(errors);
            }
        }
    }
}
