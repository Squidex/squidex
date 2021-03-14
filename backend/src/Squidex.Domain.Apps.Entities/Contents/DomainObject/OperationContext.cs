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
using GraphQL.Utilities;
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
        public List<ValidationError> Errors { get; } = new List<ValidationError>();

        public IServiceProvider Services { get; }

        public IAppEntity App { get; init; }

        public ISchemaEntity Schema { get; init; }

        public DomainId ContentId { get; init; }

        public ClaimsPrincipal? User { get; init; }

        public Func<ContentDomainObject.State> ContentProvider { get; init; }

        public ContentDomainObject.State Content
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

            Services = serviceProvider;
        }

        public static async Task<OperationContext> CreateAsync(IServiceProvider services, ContentCommand command, Func<ContentDomainObject.State> snapshot)
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

            return new OperationContext(services)
            {
                App = app,
                ContentProvider = snapshot,
                ContentId = command.ContentId,
                Schema = schema,
                User = command.User
            };
        }

        public void AddError(string message, params string[] propertyNames)
        {
            Errors.Add(new ValidationError(message, propertyNames));
        }

        public void ThrowOnErrors()
        {
            if (Errors.Count > 0)
            {
                throw new ValidationException(Errors);
            }
        }
    }
}
