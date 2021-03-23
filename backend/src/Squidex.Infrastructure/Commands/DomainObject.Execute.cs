// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public partial class DomainObject<T>
    {
        protected Task<CommandResult> CreateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : ICommand
        {
            EnsureCanCreate(command);

            return UpsertCoreAsync(command, handler, true);
        }

        protected Task<CommandResult> CreateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : ICommand
        {
            return CreateReturnAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            });
        }

        protected Task<CommandResult> CreateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            EnsureCanCreate(command);

            return UpsertCoreAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, true);
        }

        protected Task<CommandResult> Create<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : ICommand
        {
            return CreateAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            });
        }

        protected async Task<CommandResult> UpdateReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : ICommand
        {
            await EnsureCanUpdateAsync(command);

            return await UpsertCoreAsync(command, handler);
        }

        protected Task<CommandResult> UpdateReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : ICommand
        {
            return UpdateReturnAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            });
        }

        protected async Task<CommandResult> UpdateAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            await EnsureCanUpdateAsync(command);

            return await UpsertCoreAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, true);
        }

        protected async Task<CommandResult> Update<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : ICommand
        {
            return await UpdateAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            });
        }

        protected async Task<CommandResult> UpsertReturnAsync<TCommand>(TCommand command, Func<TCommand, Task<object?>> handler) where TCommand : ICommand
        {
            await EnsureCanUpsertAsync(command);

            return await UpsertCoreAsync(command, handler, true);
        }

        protected async Task<CommandResult> UpsertReturn<TCommand>(TCommand command, Func<TCommand, object?> handler) where TCommand : ICommand
        {
            return await UpsertReturnAsync(command, x =>
            {
                var result = handler(x);

                return Task.FromResult(result);
            });
        }

        protected async Task<CommandResult> UpsertAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            await EnsureCanUpsertAsync(command);

            return await UpsertCoreAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            }, true);
        }

        protected async Task<CommandResult> Upsert<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : ICommand
        {
            Guard.NotNull(handler, nameof(handler));

            return await UpsertAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            });
        }

        protected async Task<CommandResult> DeletePermanentAsync<TCommand>(TCommand command, Func<TCommand, Task> handler) where TCommand : ICommand
        {
            Guard.NotNull(handler, nameof(handler));

            await EnsureCanDeleteAsync(command);

            return await DeleteCoreAsync(command, async x =>
            {
                await handler(x);

                return None.Value;
            });
        }

        protected async Task<CommandResult> DeletePermanent<TCommand>(TCommand command, Action<TCommand> handler) where TCommand : ICommand
        {
            Guard.NotNull(handler, nameof(handler));

            return await DeletePermanentAsync(command, x =>
            {
                handler(x);

                return Task.FromResult<object?>(None.Value);
            });
        }

        private void EnsureCanCreate<TCommand>(TCommand command) where TCommand : ICommand
        {
            Guard.NotNull(command, nameof(command));

            if (Version != EtagVersion.Empty && !(IsDeleted() && CanRecreate()))
            {
                throw new DomainObjectConflictException(uniqueId.ToString());
            }

            MatchingVersion(command);
            MatchingCreateCommand(command);
        }

        private async Task EnsureCanUpdateAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            Guard.NotNull(command, nameof(command));

            await EnsureLoadedAsync();

            NotDeleted();
            NotEmpty();

            MatchingVersion(command);
            MatchingCommand(command);
        }

        private async Task EnsureCanUpsertAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            Guard.NotNull(command, nameof(command));

            await EnsureLoadedAsync();

            if (IsDeleted() && !CanRecreate())
            {
                throw new DomainObjectDeletedException(uniqueId.ToString());
            }

            MatchingVersion(command);

            if (Version <= EtagVersion.Empty)
            {
                MatchingCreateCommand(command);
            }
            else
            {
                MatchingCommand(command);
            }
        }

        private async Task EnsureCanDeleteAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            Guard.NotNull(command, nameof(command));

            await EnsureLoadedAsync();

            NotEmpty();

            MatchingVersion(command);
            MatchingCommand(command);
        }

        private void NotDeleted()
        {
            if (IsDeleted())
            {
                throw new DomainObjectDeletedException(uniqueId.ToString());
            }
        }

        private void NotEmpty()
        {
            if (Version <= EtagVersion.Empty)
            {
                throw new DomainObjectNotFoundException(uniqueId.ToString());
            }
        }

        private void MatchingVersion<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (Version > EtagVersion.Empty && command.ExpectedVersion > EtagVersion.Any && Version != command.ExpectedVersion)
            {
                throw new DomainObjectVersionException(uniqueId.ToString(), Version, command.ExpectedVersion);
            }
        }

        private void MatchingCreateCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (!CanAcceptCreation(command))
            {
                throw new DomainException("Invalid command.");
            }
        }

        private void MatchingCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (!CanAccept(command))
            {
                throw new DomainException("Invalid command.");
            }
        }
    }
}
