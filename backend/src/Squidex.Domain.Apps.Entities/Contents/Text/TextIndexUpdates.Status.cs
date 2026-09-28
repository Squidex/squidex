// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

internal sealed partial class TextIndexUpdates
{
    private void CreateDraft(ContentEvent @event)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            switch (state.State)
            {
                case TextState.Stage0_Published__Stage1_None:
                    state.State = TextState.Stage0_Published__Stage1_Draft;
                    break;
                case TextState.Stage1_Published__Stage0_None:
                    state.State = TextState.Stage1_Published__Stage0_Draft;
                    break;
            }

            currentUpdates[state.UniqueContentId] = state;
        }
    }

    private void Unpublish(ContentEvent @event)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            switch (state.State)
            {
                case TextState.Stage0_Published__Stage1_None:
                    commands.Update(@event, uniqueId, 0, true, false);

                    state.State = TextState.Stage0_Draft__Stage1_None;
                    break;
                case TextState.Stage1_Published__Stage0_None:
                    commands.Update(@event, uniqueId, 1, true, false);

                    state.State = TextState.Stage1_Draft__Stage0_None;
                    break;
            }

            currentUpdates[state.UniqueContentId] = state;
        }
    }

    private void Publish(ContentEvent @event)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            switch (state.State)
            {
                case TextState.Stage0_Published__Stage1_Draft:
                    commands.Update(@event, uniqueId, 1, true, true);
                    commands.Delete(@event, uniqueId, 0);

                    state.State = TextState.Stage1_Published__Stage0_None;
                    break;
                case TextState.Stage1_Published__Stage0_Draft:
                    commands.Update(@event, uniqueId, 0, true, true);
                    commands.Delete(@event, uniqueId, 1);

                    state.State = TextState.Stage0_Published__Stage1_None;
                    break;
                case TextState.Stage0_Draft__Stage1_None:
                    commands.Update(@event, uniqueId, 0, true, true);

                    state.State = TextState.Stage0_Published__Stage1_None;
                    break;
                case TextState.Stage1_Draft__Stage0_None:
                    commands.Update(@event, uniqueId, 1, true, true);

                    state.State = TextState.Stage1_Published__Stage0_None;
                    break;
            }

            currentUpdates[state.UniqueContentId] = state;
        }
    }

    private void DeleteDraft(ContentEvent @event)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            switch (state.State)
            {
                case TextState.Stage0_Published__Stage1_Draft:
                    commands.Update(@event, uniqueId, 0, true, true);
                    commands.Delete(@event, uniqueId, 1);

                    state.State = TextState.Stage0_Published__Stage1_None;
                    break;
                case TextState.Stage1_Published__Stage0_Draft:
                    commands.Update(@event, uniqueId, 1, true, true);
                    commands.Delete(@event, uniqueId, 0);

                    state.State = TextState.Stage1_Published__Stage0_None;
                    break;
            }

            currentUpdates[state.UniqueContentId] = state;
        }
    }

    private void Delete(ContentEvent @event)
    {
        var uniqueId = new UniqueContentId(@event.AppId.Id, @event.ContentId);
        if (states.TryGetValue(uniqueId, out var state))
        {
            commands.Delete(@event, uniqueId, 0);
            commands.Delete(@event, uniqueId, 1);

            state.State = TextState.Deleted;

            currentUpdates[state.UniqueContentId] = state;
        }
    }
}
