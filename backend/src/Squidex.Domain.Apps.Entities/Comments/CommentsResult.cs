// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Comments;

public sealed class CommentsResult
{
    public List<Comment> CreatedComments { get; set; } = new List<Comment>();

    public List<Comment> UpdatedComments { get; set; } = new List<Comment>();

    public List<DomainId> DeletedComments { get; set; } = new List<DomainId>();

    public long Version { get; set; }

    public static CommentsResult FromEvents(IEnumerable<Envelope<CommentsEvent>> events, long currentVersion, int lastVersion)
    {
        var result = new CommentsResult { Version = currentVersion };

        foreach (var @event in events.Skip(lastVersion < 0 ? 0 : lastVersion + 1))
        {
            switch (@event.Payload)
            {
                case CommentDeleted deleted:
                    {
                        var id = deleted.CommentId;

                        if (result.CreatedComments.Any(x => x.Id == id))
                        {
                            result.CreatedComments.RemoveAll(x => x.Id == id);
                        }
                        else if (result.UpdatedComments.Any(x => x.Id == id))
                        {
                            result.UpdatedComments.RemoveAll(x => x.Id == id);
                            result.DeletedComments.Add(id);
                        }
                        else
                        {
                            result.DeletedComments.Add(id);
                        }

                        break;
                    }

                case CommentCreated created:
                    {
                        var comment = new Comment(
                            created.CommentId,
                            @event.Headers.Timestamp(),
                            @event.Payload.Actor,
                            created.Text,
                            created.Url);

                        result.CreatedComments.Add(comment);
                        break;
                    }

                case CommentUpdated updated:
                    {
                        var id = updated.CommentId;

                        var comment = new Comment(
                            id,
                            @event.Headers.Timestamp(),
                            @event.Payload.Actor,
                            updated.Text,
                            null);

                        if (result.CreatedComments.Any(x => x.Id == id))
                        {
                            result.CreatedComments.RemoveAll(x => x.Id == id);
                            result.CreatedComments.Add(comment);
                        }
                        else
                        {
                            result.UpdatedComments.Add(comment);
                        }

                        break;
                    }
            }
        }

        return result;
    }
}
