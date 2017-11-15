// ==========================================================================
//  AppStateGrainState_Apps.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Apps.Utils;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed partial class AppStateGrainState
    {
        public void On(AppCreated @event, EnvelopeHeaders headers)
        {
            Reset();

            App = EntityMapper.Create<JsonAppEntity>(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);

                a.LanguagesConfig = LanguagesConfig.Build(Language.EN);
            });
        }

        public void On(AppLanguageAdded @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.LanguagesConfig = a.LanguagesConfig.Apply(@event);
            });
        }

        public void On(AppLanguageRemoved @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.LanguagesConfig = a.LanguagesConfig.Apply(@event);
            });
        }

        public void On(AppLanguageUpdated @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.LanguagesConfig = a.LanguagesConfig.Apply(@event);
            });
        }

        public void On(AppContributorAssigned @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Contributors = a.Contributors.Apply(@event);
            });
        }

        public void On(AppContributorRemoved @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Contributors = a.Contributors.Apply(@event);
            });
        }

        public void On(AppClientAttached @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Clients = a.Clients.Apply(@event);
            });
        }

        public void On(AppClientUpdated @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Clients = a.Clients.Apply(@event);
            });
        }

        public void On(AppClientRenamed @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Clients = a.Clients.Apply(@event);
            });
        }

        public void On(AppClientRevoked @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                a.Clients = a.Clients.Apply(@event);
            });
        }

        public void On(AppPlanChanged @event, EnvelopeHeaders headers)
        {
            UpdateApp(@event, headers, a =>
            {
                SimpleMapper.Map(@event, a);
            });
        }

        private void UpdateApp(AppEvent @event, EnvelopeHeaders headers, Action<JsonAppEntity> updater = null)
        {
            App = App.Clone().Update(@event, headers, updater);
        }
    }
}
