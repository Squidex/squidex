// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities;

public abstract class OperationContextBase<TCommand, TSnapShot> where TCommand : SquidexCommand, IAggregateCommand
{
    private readonly List<ValidationError> errors = new List<ValidationError>();
    private readonly IServiceProvider serviceProvider;
    private readonly Func<TSnapShot> snapshotProvider;
    private readonly TSnapShot snapshotInitial;

    public RefToken Actor => Command.Actor;

    public IAppEntity App { get; init; }

    public DomainId CommandId { get; init; }

    public TCommand Command { get; init; }

    public TSnapShot Snapshot => snapshotProvider();

    public TSnapShot SnapshotInitial => snapshotInitial;

    public ClaimsPrincipal? User => Command.User;

    public Dictionary<string, object> Context { get; } = new Dictionary<string, object>();

    protected OperationContextBase(IServiceProvider serviceProvider, Func<TSnapShot> snapshotProvider)
    {
        Guard.NotNull(serviceProvider);
        Guard.NotNull(snapshotProvider);

        this.serviceProvider = serviceProvider;
        this.snapshotProvider = snapshotProvider;
        this.snapshotInitial = snapshotProvider();
    }

    public T Resolve<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }

    public T? ResolveOptional<T>() where T : class
    {
        return serviceProvider.GetService(typeof(T)) as T;
    }

    public OperationContextBase<TCommand, TSnapShot> AddError(string message, params string[] propertyNames)
    {
        errors.Add(new ValidationError(message, propertyNames));

        return this;
    }

    public OperationContextBase<TCommand, TSnapShot> AddError(ValidationError newError)
    {
        errors.Add(newError);

        return this;
    }

    public OperationContextBase<TCommand, TSnapShot> AddErrors(IEnumerable<ValidationError> newErrors)
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
