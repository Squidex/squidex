﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Apps.State
{
    [CollectionName("Apps")]
    public class AppState : DomainObjectState<AppState>, IAppEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Roles Roles { get; set; } = Roles.Empty;

        [DataMember]
        public AppPlan? Plan { get; set; }

        [DataMember]
        public AppImage? Image { get; set; }

        [DataMember]
        public AppClients Clients { get; set; } = AppClients.Empty;

        [DataMember]
        public AppPatterns Patterns { get; set; } = AppPatterns.Empty;

        [DataMember]
        public AppContributors Contributors { get; set; } = AppContributors.Empty;

        [DataMember]
        public LanguagesConfig LanguagesConfig { get; set; } = LanguagesConfig.English;

        [DataMember]
        public Workflows Workflows { get; set; } = Workflows.Empty;

        [DataMember]
        public bool IsArchived { get; set; }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AppCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AppUpdated e when !string.Equals(e.Label, Label) || !string.Equals(e.Description, Description):
                    {
                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AppImageUploaded e:
                    return UpdateImage(e, ev => ev.Image);

                case AppImageRemoved e when Image != null:
                    return UpdateImage(e, ev => null);

                case AppPlanChanged e when !string.Equals(Plan?.PlanId, e.PlanId):
                    return UpdatePlan(e, ev => AppPlan.Build(ev.Actor, ev.PlanId));

                case AppPlanReset e when Plan != null:
                    return UpdatePlan(e, ev => null);

                case AppContributorAssigned e:
                    return UpdateContributors(e, (ev, c) => c.Assign(ev.ContributorId, ev.Role));

                case AppContributorRemoved e:
                    return UpdateContributors(e, (ev, c) => c.Remove(ev.ContributorId));

                case AppClientAttached e:
                    return UpdateClients(e, (ev, c) => c.Add(ev.Id, ev.Secret));

                case AppClientUpdated e:
                    return UpdateClients(e, (ev, c) => c.Update(ev.Id, ev.Role));

                case AppClientRenamed e:
                    return UpdateClients(e, (ev, c) => c.Rename(ev.Id, ev.Name));

                case AppClientRevoked e:
                    return UpdateClients(e, (ev, c) => c.Revoke(ev.Id));

                case AppWorkflowAdded e:
                    return UpdateWorkflows(e, (ev, w) => w.Add(ev.WorkflowId, ev.Name));

                case AppWorkflowUpdated e:
                    return UpdateWorkflows(e, (ev, w) => w.Update(ev.WorkflowId, ev.Workflow));

                case AppWorkflowDeleted e:
                    return UpdateWorkflows(e, (ev, w) => w.Remove(ev.WorkflowId));

                case AppPatternAdded e:
                    return UpdatePatterns(e, (ev, p) => p.Add(ev.PatternId, ev.Name, ev.Pattern, ev.Message));

                case AppPatternDeleted e:
                    return UpdatePatterns(e, (ev, p) => p.Remove(ev.PatternId));

                case AppPatternUpdated e:
                    return UpdatePatterns(e, (ev, p) => p.Update(ev.PatternId, ev.Name, ev.Pattern, ev.Message));

                case AppRoleAdded e:
                    return UpdateRoles(e, (ev, r) => r.Add(ev.Name));

                case AppRoleUpdated e:
                    return UpdateRoles(e, (ev, r) => r.Update(ev.Name, ev.Permissions));

                case AppRoleDeleted e:
                    return UpdateRoles(e, (ev, r) => r.Remove(ev.Name));

                case AppLanguageAdded e:
                    return UpdateLanguages(e, (ev, l) => l.Set(ev.Language));

                case AppLanguageRemoved e:
                    return UpdateLanguages(e, (ev, l) => l.Remove(ev.Language));

                case AppLanguageUpdated e:
                    return UpdateLanguages(e, (ev, l) =>
                    {
                        l = l.Set(ev.Language, ev.IsOptional, ev.Fallback);

                        if (ev.IsMaster)
                        {
                            LanguagesConfig = LanguagesConfig.MakeMaster(ev.Language);
                        }

                        return l;
                    });

                case AppArchived _:
                    {
                        Plan = null;

                        IsArchived = true;

                        return true;
                    }
            }

            return false;
        }

        private bool UpdateContributors<T>(T @event, Func<T, AppContributors, AppContributors> update)
        {
            var previous = Contributors;

            Contributors = update(@event, previous);

            return !ReferenceEquals(previous, Contributors);
        }

        private bool UpdateClients<T>(T @event, Func<T, AppClients, AppClients> update)
        {
            var previous = Clients;

            Clients = update(@event, previous);

            return !ReferenceEquals(previous, Clients);
        }

        private bool UpdateLanguages<T>(T @event, Func<T, LanguagesConfig, LanguagesConfig> update)
        {
            var previous = LanguagesConfig;

            LanguagesConfig = update(@event, previous);

            return !ReferenceEquals(previous, LanguagesConfig);
        }

        private bool UpdatePatterns<T>(T @event, Func<T, AppPatterns, AppPatterns> update)
        {
            var previous = Patterns;

            Patterns = update(@event, previous);

            return !ReferenceEquals(previous, Patterns);
        }

        private bool UpdateRoles<T>(T @event, Func<T, Roles, Roles> update)
        {
            var previous = Roles;

            Roles = update(@event, previous);

            return !ReferenceEquals(previous, Roles);
        }

        private bool UpdateWorkflows<T>(T @event, Func<T, Workflows, Workflows> update)
        {
            var previous = Workflows;

            Workflows = update(@event, previous);

            return !ReferenceEquals(previous, Workflows);
        }

        private bool UpdateImage<T>(T @event, Func<T, AppImage?> update)
        {
            Image = update(@event);

            return true;
        }

        private bool UpdatePlan<T>(T @event, Func<T, AppPlan?> update)
        {
            Plan = update(@event);

            return true;
        }
    }
}
