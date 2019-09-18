using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.ICIS.Validation
{
    public sealed class CommentaryCommandMiddleware : ICustomCommandMiddleware
    {
        private readonly IEnumerable<ICommentaryValidator> validators;
        private readonly IContextProvider contextProvider;
        private readonly IGrainFactory grainFactory;

        public CommentaryCommandMiddleware(
            IEnumerable<ICommentaryValidator> validators,
            IContextProvider contextProvider, 
            IGrainFactory grainFactory)
        {
            this.validators = validators;
            this.contextProvider = contextProvider;
            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateContent createContent)
            {
                await ValidateContentAsync(createContent.ContentId, createContent.Data, createContent.SchemaId);
            }
            else if (context.Command is UpdateContent updateContent)
            {
                var content = await grainFactory.GetGrain<IContentGrain>(updateContent.ContentId).GetStateAsync();

                await ValidateContentAsync(updateContent.ContentId, updateContent.Data, content.Value.SchemaId);
            }

            await next();
        }

        private async Task ValidateContentAsync(Guid contentId, NamedContentData data, NamedId<Guid> schemaId)
        {
            var errors = await Task.WhenAll(validators.Select(v => v.ValidateCommentaryAsync(contentId, schemaId, contextProvider.Context, data)));

            var combinedErrors = errors.Where(x => x != null).SelectMany(x => x).ToList();

            if (combinedErrors.Count > 0)
            {
                throw new ValidationException("Failed to save commentary.", combinedErrors);
            }
        }
    }
}
